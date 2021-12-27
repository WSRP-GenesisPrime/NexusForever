using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using NLog;
using System;

namespace NexusForever.WorldServer.Game.Spell
{
    [SpellType(CastMethod.Channeled)]
    public partial class SpellChanneled : Spell, ISpell
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static CastMethod castMethod = CastMethod.Channeled;

        public SpellChanneled(UnitEntity caster, SpellParameters parameters)
            : base(caster, parameters, castMethod)
        {
        }

        public override bool Cast()
        {
            if (!base.Cast())
                return false;

            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.ChannelInitialDelay / 1000d, () =>
            {
                CastResult checkResources = CheckResourceConditions();
                if (checkResources != CastResult.Ok)
                {
                    CancelCast(checkResources);
                    return;
                }

                Execute();

                targets.ForEach(t => t.Effects.Clear());
            })); // Execute after initial delay
            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.ChannelMaxTime / 1000d, Finish)); // End Spell Cast

            uint numberOfPulses = (uint)MathF.Floor(parameters.SpellInfo.Entry.ChannelMaxTime / parameters.SpellInfo.Entry.ChannelPulseTime); // Calculate number of "ticks" in this spell cast

            // Add ticks at each pulse
            for (int i = 1; i <= numberOfPulses; i++)
                events.EnqueueEvent(new SpellEvent((parameters.SpellInfo.Entry.ChannelInitialDelay + (parameters.SpellInfo.Entry.ChannelPulseTime * i)) / 1000d, () =>
                {
                    CastResult checkResources = CheckResourceConditions();
                    if (checkResources != CastResult.Ok)
                    {
                        CancelCast(checkResources);
                        return;
                    }

                    effectTriggerCount.Clear();
                    Execute();

                    targets.ForEach(t => t.Effects.Clear());
                }));

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && (status == SpellStatus.Casting || status == SpellStatus.Executing);
        }
    }
}
