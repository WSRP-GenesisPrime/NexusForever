﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Prerequisite;
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
        public SpellInfo AlternateSpellInfo { get; private set; }
        public ItemEntity Item { get; }
        public uint GlobalCooldownEnum => SpellInfo.Entry.GlobalCooldownEnum;

        public SpellBaseInfo PetAbilityBaseInfo { get; private set; }
        public bool HasPetAbility => SpellInfo.Entry.Spell4IdPetSwitch > 0;
        public bool IsPetActive => HasPetAbility && petUnitId > 0;
        
        private uint petUnitId;

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
        private CastMethod castMethod;
        private bool noCooldown => SpellInfo.Entry.SpellCoolDown == 0 && SpellInfo.Entry.SpellCoolDownIds.Where(x => x != 0).Count() == 0u;

        /// <summary>
        /// Create a new <see cref="CharacterSpell"/> from an existing database model.
        /// </summary>
        public CharacterSpell(Player player, CharacterSpellModel model, SpellBaseInfo baseInfo, ItemEntity item)
        {
            Owner      = player;
            BaseInfo   = baseInfo;
            SpellInfo  = baseInfo.GetSpellInfo(tier);
            Item       = item;
            tier       = model.Tier;
            castMethod = (CastMethod)baseInfo.Entry.CastMethod;
            if (SpellInfo.Entry.Spell4IdMechanicAlternateSpell > 0)
            {
                Spell4Entry alternativeEntry = GameTableManager.Instance.Spell4.GetEntry(SpellInfo.Entry.Spell4IdMechanicAlternateSpell);
                AlternateSpellInfo = GlobalSpellManager.Instance.GetSpellBaseInfo(alternativeEntry.Spell4BaseIdBaseSpell).GetSpellInfo(tier);
            }

            InitialiseAbilityCharges();
            InitialisePetAbility();
        }

        /// <summary>
        /// Create a new <see cref="CharacterSpell"/> from a <see cref="SpellBaseInfo"/>.
        /// </summary>
        public CharacterSpell(Player player, SpellBaseInfo baseInfo, byte tier, ItemEntity item)
        {
            Owner      = player;
            BaseInfo   = baseInfo ?? throw new ArgumentNullException();
            SpellInfo  = baseInfo.GetSpellInfo(tier);
            Item       = item;
            this.tier  = tier;
            castMethod = (CastMethod)baseInfo.Entry.CastMethod;

            InitialiseAbilityCharges();
            InitialisePetAbility();

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

            buttonPressed = true;
            CastSpell();
            buttonPressed = false;
        }

        /// <summary>
        /// Used for continuous casting when the client has it enabled, or spells with Cast Methods like ChargeRelease
        /// </summary>
        public void Cast(bool buttonPressed)
        {
            // TODO: Handle continuous casting of spell for Player if button remains depressed
            this.buttonPressed = buttonPressed;

            // If the player depresses button after the spell had exceeded its threshold, don't try and recast the spell until button is pressed down again.
            if (buttonPressed && castMethod != CastMethod.ChargeRelease)
                Owner.SpellManager.SetAsContinuousCast(this);
            else if (!buttonPressed && castMethod != CastMethod.ChargeRelease)
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
            var spellInfoToCast = SpellInfo;
            if (AlternateSpellInfo != null && CheckRunnerOverride())
                spellInfoToCast = AlternateSpellInfo;

            if (HasPetAbility && IsPetActive)
            {
                CharacterSpell characterSpell = Owner.SpellManager.GetSpell(PetAbilityBaseInfo.Entry.Id);
                if (characterSpell == null)
                    throw new InvalidPacketValueException();

                characterSpell.Cast();
                return;
            }

            // For Threshold Spells
            if (Owner.HasSpell(spellInfoToCast.Entry.Id, out Spell spell, isCasting: castMethod == CastMethod.ChargeRelease))
            {
                if ((spell.CastMethod == CastMethod.RapidTap || spell.CastMethod == CastMethod.ChargeRelease) && !spell.IsFinished)
                {
                    spell.Cast();
                    return;
                }
            }

            if (!buttonPressed)
                return;

            Owner.CastSpell(new SpellParameters
            {
                CharacterSpell         = this,
                RootSpellInfo          = SpellInfo,
                SpellInfo              = spellInfoToCast,
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

        private void InitialisePetAbility()
        {
            if (SpellInfo.Entry.Spell4IdPetSwitch == 0)
                return;

            Spell4Entry spell4Entry = GameTableManager.Instance.Spell4.GetEntry(SpellInfo.Entry.Spell4IdPetSwitch);
            if (spell4Entry == null)
                throw new ArgumentOutOfRangeException();

            SpellBaseInfo baseInfo = GlobalSpellManager.Instance.GetSpellBaseInfo(spell4Entry.Spell4BaseIdBaseSpell);
            if (baseInfo == null)
                throw new ArgumentOutOfRangeException();

            PetAbilityBaseInfo = baseInfo;
        }

        public void SetPetUnitId(uint unitId)
        {
            if (!HasPetAbility)
                throw new InvalidOperationException();

            petUnitId = unitId;
        }

        private bool CheckRunnerOverride()
        {
            foreach (PrerequisiteEntry runnerPrereq in SpellInfo.PrerequisiteRunners)
                if (PrerequisiteManager.Instance.Meets(Owner, runnerPrereq.Id))
                    return true;

            return false;
        }
    }
}
