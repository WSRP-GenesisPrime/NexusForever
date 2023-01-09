using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Game.Spell
{
    public partial class SpellThreshold : Spell, ISpell
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public bool HasThresholdToCast => (parameters.SpellInfo.Thresholds.Count > 0 && thresholdValue < thresholdMax) || thresholdSpells.Count > 0;

        private readonly List<Spell> thresholdSpells = new List<Spell>();
        private double holdDuration;
        protected uint totalThresholdTimer;
        protected uint thresholdMax;
        protected uint thresholdValue;

        public SpellThreshold(UnitEntity caster, SpellParameters parameters, CastMethod castMethod)
            : base(caster, parameters, castMethod)
        {
            thresholdMax = (uint)parameters.SpellInfo.Thresholds.Count;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (status == SpellStatus.Initiating)
                return;

            if (status == SpellStatus.Executing && HasThresholdToCast)
                status = SpellStatus.Waiting;

            if (status == SpellStatus.Waiting && CastMethod == CastMethod.ChargeRelease)
            {
                holdDuration += lastTick;

                if (holdDuration >= totalThresholdTimer)
                    HandleThresholdCast();
            }

            thresholdSpells.ForEach(s => s.Update(lastTick));
            thresholdSpells.ForEach(s => s.LateUpdate(lastTick));
            if (status == SpellStatus.Waiting && HasThresholdToCast)
            {
                foreach (Spell thresholdSpell in thresholdSpells.ToList())
                    if (thresholdSpell.IsFinished)
                        thresholdSpells.Remove(thresholdSpell);
            }
        }

        public override bool Cast()
        {
            if (status == SpellStatus.Waiting)
                return HandleThresholdCast();

            if (!base.Cast())
                return false;

            return true;
        }

        protected virtual void Execute()
        {
            
            if ((currentPhase == 0 || currentPhase == 255) && !HasThresholdToCast && CastMethod != CastMethod.ChargeRelease)
            {
                CostSpell();
                SetCooldown();
            }

            base.Execute(false);

            // TODO: Confirm whether RapidTap spells cancel another out, and add logic as necessary

            if (parameters.SpellInfo.Entry.ThresholdTime > 0)
                SendThresholdStart();

            if (parameters.ThresholdValue > 0 && parameters.RootSpellInfo.Thresholds.Count > 1)
                SendThresholdUpdate();
        }

        private bool HandleThresholdCast()
        {
            if (status != SpellStatus.Waiting)
                throw new InvalidOperationException();

            if (parameters.SpellInfo.Thresholds.Count == 0)
                throw new InvalidOperationException();

            CastResult result = CheckCast();
            if (result != CastResult.Ok)
            {
                if (CastMethod == CastMethod.RapidTap && result != CastResult.PrereqCasterCast)
                {
                    if (caster is Player player)
                        player.SpellManager.SetAsContinuousCast(null);

                    SendSpellCastResult(result);
                    return false;
                }
            }

            Spell thresholdSpell = InitialiseThresholdSpell();
            thresholdSpell.Cast();
            thresholdSpells.Add(thresholdSpell);

            switch (CastMethod)
            {
                case CastMethod.ChargeRelease:
                    SetCooldown();
                    thresholdValue = thresholdMax;
                    status = SpellStatus.Finishing;
                    break;
                case CastMethod.RapidTap:
                    thresholdValue++;
                    break;
            }

            return true;
        }

        private Spell InitialiseThresholdSpell()
        {
            if (parameters.SpellInfo.Thresholds.Count == 0)
                return null;

            (SpellInfo spellInfo, Spell4ThresholdsEntry thresholdsEntry) = parameters.SpellInfo.GetThresholdSpellInfo((int)thresholdValue);
            if (spellInfo == null || thresholdsEntry == null)
                throw new InvalidOperationException($"{spellInfo} or {thresholdsEntry} is null!");

            Spell thresholdSpell = GlobalSpellManager.Instance.NewSpell((CastMethod)spellInfo.BaseInfo.Entry.CastMethod, caster, new SpellParameters
            {
                SpellInfo = spellInfo,
                ParentSpellInfo = parameters.SpellInfo,
                RootSpellInfo = parameters.SpellInfo,
                UserInitiatedSpellCast = parameters.UserInitiatedSpellCast,
                ThresholdValue = thresholdsEntry.OrderIndex + 1,
                IsProxy = CastMethod == CastMethod.ChargeRelease
            });

            log.Trace($"Added Child Spell {thresholdSpell.Spell4Id} with casting ID {thresholdSpell.CastingId} to parent casting ID {CastingId}");

            return thresholdSpell;
        }

        public override void CancelCast(CastResult result)
        {
            if (!IsCasting && !HasThresholdToCast)
                return;

            if (HasThresholdToCast && thresholdSpells.Count > 0)
                if (thresholdSpells[0].IsCasting)
                {
                    thresholdSpells[0].CancelCast(result);
                    return;
                }

            base.CancelCast(result);
        }

        public override void Finish()
        {
            if (status == SpellStatus.Finished)
                return;

            thresholdValue = thresholdMax;
            base.Finish();
        }

        private void SendThresholdStart()
        {
            if (caster is Player player)
                player.Session.EnqueueMessageEncrypted(new ServerSpellThresholdStart
                {
                    Spell4Id = parameters.SpellInfo.Entry.Id,
                    RootSpell4Id = parameters.RootSpellInfo?.Entry.Id ?? 0,
                    ParentSpell4Id = parameters.ParentSpellInfo?.Entry.Id ?? 0,
                    CastingId = CastingId
                });
        }

        protected void SendThresholdUpdate()
        {
            if (caster is Player player)
                player.Session.EnqueueMessageEncrypted(new ServerSpellThresholdUpdate
                {
                    Spell4Id = parameters.ParentSpellInfo?.Entry.Id ?? Spell4Id,
                    Value = parameters.ThresholdValue > 0 ? (byte)parameters.ThresholdValue : (byte)thresholdValue
                });
        }

        protected override bool CanFinish()
        {
            return (status == SpellStatus.Waiting && !HasThresholdToCast) || base.CanFinish();
        }

        protected override void OnStatusChange(SpellStatus previousStatus, SpellStatus status)
        {
            base.OnStatusChange(previousStatus, status);

            if (status == SpellStatus.Finished && caster is Player player)
            {
                if (thresholdMax > 0)
                {
                    player.Session.EnqueueMessageEncrypted(new ServerSpellThresholdClear
                    {
                        Spell4Id = Spell4Id
                    });

                    if (CastMethod != CastMethod.ChargeRelease)
                        SetCooldown();
                }
            }
        }
    }
}
