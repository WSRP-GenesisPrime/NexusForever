using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Spell
{
    [SpellType(CastMethod.RapidTap)]
    public partial class SpellRapidTap : SpellThreshold, ISpell
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static CastMethod castMethod = CastMethod.RapidTap;

        public SpellRapidTap(UnitEntity caster, SpellParameters parameters) 
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
                events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d + parameters.SpellInfo.Entry.ThresholdTime / 1000d, Finish)); // enqueue spell to be executed after cast time

            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && status == SpellStatus.Casting;
        }
    }
}
