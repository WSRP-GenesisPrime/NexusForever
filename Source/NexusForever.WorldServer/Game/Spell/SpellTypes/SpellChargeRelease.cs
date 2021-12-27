using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Spell
{
    [SpellType(CastMethod.ChargeRelease)]
    public partial class SpellChargeRelease : SpellThreshold, ISpell
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static CastMethod castMethod = CastMethod.ChargeRelease;

        public SpellChargeRelease(UnitEntity caster, SpellParameters parameters) 
            : base(caster, parameters, castMethod)
        {
        }

        public override bool Cast()
        {
            if (status == SpellStatus.Waiting)
                return base.Cast();

            if (!base.Cast())
                return false;

            if (parameters.ParentSpellInfo == null)
            {
                totalThresholdTimer = (uint)(parameters.SpellInfo.Entry.ThresholdTime / 1000d);

                // Keep track of cast time increments as we create timers to adjust thresholdValue
                uint nextCastTime = 0;

                // Create timers for each thresholdEntry's timer increment
                foreach (Spell4ThresholdsEntry thresholdsEntry in parameters.SpellInfo.Thresholds)
                {
                    nextCastTime += thresholdsEntry.ThresholdDuration;

                    if (thresholdsEntry.OrderIndex == 0)
                        continue;

                    events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d + nextCastTime / 1000d, () =>
                    {
                        thresholdValue = thresholdsEntry.OrderIndex;
                        SendThresholdUpdate();
                    }));
                }
            }

            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && (status == SpellStatus.Casting || status == SpellStatus.Executing || status == SpellStatus.Waiting); ;
        }
    }
}
