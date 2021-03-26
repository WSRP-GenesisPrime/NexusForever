﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network.Message.Model;
using ItemEntity = NexusForever.WorldServer.Game.Entity.Item;

namespace NexusForever.WorldServer.Game.Spell
{
    public class CharacterSpell : ISaveCharacter, IUpdate
    {
        public Player Owner { get; }
        public SpellBaseInfo BaseInfo { get; }
        public SpellInfo SpellInfo { get; private set; }
        public ItemEntity Item { get; }

        public byte Tier
        {
            get => tier;
            set
            {
                if (tier != value)
                    SpellInfo = BaseInfo.GetSpellInfo(tier);

                tier = value;
                saveMask |= UnlockedSpellSaveMask.Tier;
            }
        }
        private byte tier;

        public uint AbilityCharges { get; private set; }
        public uint MaxAbilityCharges => SpellInfo.Entry.AbilityChargeCount;

        private UnlockedSpellSaveMask saveMask;

        private UpdateTimer rechargeTimer;
        private bool buttonPressed;
        private bool noCooldown => SpellInfo.Entry.SpellCoolDown == 0 && SpellInfo.Entry.SpellCoolDownIds.Where(x => x != 0).Count() == 0u;

        /// <summary>
        /// Create a new <see cref="CharacterSpell"/> from an existing database model.
        /// </summary>
        public CharacterSpell(Player player, CharacterSpellModel model, SpellBaseInfo baseInfo, ItemEntity item)
        {
            Owner     = player;
            BaseInfo  = baseInfo;
            SpellInfo = baseInfo.GetSpellInfo(tier);
            Item      = item;
            tier      = model.Tier;

            InitialiseAbilityCharges();
        }

        /// <summary>
        /// Create a new <see cref="CharacterSpell"/> from a <see cref="SpellBaseInfo"/>.
        /// </summary>
        public CharacterSpell(Player player, SpellBaseInfo baseInfo, byte tier, ItemEntity item)
        {
            Owner     = player;
            BaseInfo  = baseInfo ?? throw new ArgumentNullException();
            SpellInfo = baseInfo.GetSpellInfo(tier);
            Item      = item;
            this.tier = tier;

            InitialiseAbilityCharges();

            saveMask = UnlockedSpellSaveMask.Create;
        }

        private void InitialiseAbilityCharges()
        {
            if (MaxAbilityCharges == 0u)
                return;

            rechargeTimer  = new(SpellInfo.Entry.AbilityRechargeTime / 1000d, false);
            AbilityCharges = MaxAbilityCharges;
            SendChargeUpdate();
        }

        public void Update(double lastTick)
        {
            //if (MaxAbilityCharges > 0 && AbilityCharges < MaxAbilityCharges)
            if (MaxAbilityCharges > 0 && rechargeTimer.IsTicking)
            {
                rechargeTimer.Update(lastTick);
                if (rechargeTimer.HasElapsed)
                {
                    AbilityCharges = Math.Clamp(AbilityCharges + SpellInfo.Entry.AbilityRechargeCount, 0u, MaxAbilityCharges);
                    SendChargeUpdate();
                    rechargeTimer.Reset(AbilityCharges < MaxAbilityCharges);
                }
            }
        }

        public void Save(CharacterContext context)
        {
            if (saveMask == UnlockedSpellSaveMask.None)
                return;

            if ((saveMask & UnlockedSpellSaveMask.Create) != 0)
            {
                var model = new CharacterSpellModel
                {
                    Id           = Owner.CharacterId,
                    Spell4BaseId = BaseInfo.Entry.Id,
                    Tier         = tier
                };

                context.Add(model);
            }
            else
            {
                var model = new CharacterSpellModel
                {
                    Id           = Owner.CharacterId,
                    Spell4BaseId = BaseInfo.Entry.Id,
                };

                EntityEntry<CharacterSpellModel> entity = context.Attach(model);
                if ((saveMask & UnlockedSpellSaveMask.Tier) != 0)
                {
                    model.Tier = tier;
                    entity.Property(p => p.Tier).IsModified = true;
                }
            }

            saveMask = UnlockedSpellSaveMask.None;
        }

        /// <summary>
        /// Used to call this spell from the <see cref="SpellManager"/>. For use in continuous casting.
        /// </summary>
        public void SpellManagerCast()
        {
            if (!buttonPressed)
                throw new InvalidOperationException($"Spell should not cast because button is not held down!");

            CastSpell();
        }

        /// <summary>
        /// Used for when the client does not have continuous casting enabled
        /// </summary>
        public void Cast()
        {
            Owner.SpellManager.SetAsContinuousCast(null);

            if (Owner.HasSpell(BaseInfo.GetSpellInfo(Tier).Entry.Id, out Spell spell))
            {
                if ((spell.CastMethod == CastMethod.RapidTap || spell.CastMethod == CastMethod.ChargeRelease) && !spell.IsFinished)
                {
                    spell.Cast();
                    return;
                }
            }

            CastSpell();
        }

        /// <summary>
        /// Used for continuous casting when the client has it enabled, or spells with Cast Methods like ChargeRelease
        /// </summary>
        public void Cast(bool buttonPressed)
        {
            // TODO: Handle continuous casting of spell for Player if button remains depressed
            this.buttonPressed = buttonPressed;

            // If the player depresses button after the spell had exceeded its threshold, don't try and recast the spell until button is pressed down again.
            if (buttonPressed && noCooldown && (CastMethod)BaseInfo.Entry.CastMethod != CastMethod.ChargeRelease)
                Owner.SpellManager.SetAsContinuousCast(this);
            else if (!buttonPressed && (CastMethod)BaseInfo.Entry.CastMethod != CastMethod.ChargeRelease)
            {
                Owner.SpellManager.SetAsContinuousCast(null);
                return;
            }
            else
                Owner.SpellManager.SetAsContinuousCast(null);

            CastSpell();
        }

        private void CastSpell()
        {
            if (Owner.HasSpell(BaseInfo.GetSpellInfo(Tier).Entry.Id, out Spell spell))
            {
                if ((spell.CastMethod == CastMethod.RapidTap || spell.CastMethod == CastMethod.ChargeRelease) && !spell.IsFinished)
                {
                    spell.Cast();
                    return;
                }
            }

            Owner.CastSpell(new SpellParameters
            {
                CharacterSpell         = this,
                SpellInfo              = SpellInfo,
                UserInitiatedSpellCast = true
            });
        }

        public void UseCharge()
        {
            if (AbilityCharges == 0)
                throw new SpellException("No charges available.");

            // TODO: Ability Charges are affected by ModifyCooldown spell effect. Needs to be handled to adjust Charge timer. Possibly move charges to SpellManager.
            AbilityCharges -= 1;
            if (!rechargeTimer.IsTicking)
                rechargeTimer.Reset(true);
            SendChargeUpdate();
        }

        private void SendChargeUpdate()
        {
            Owner.Session.EnqueueMessageEncrypted(new ServerSpellAbilityCharges
            {
                SpellId            = Item.Id,
                AbilityChargeCount = AbilityCharges
            });
        }
    }
}
