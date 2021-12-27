using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Spell
{
    [SpellType(CastMethod.Multiphase)]
    public partial class SpellMultiphase : Spell, ISpell
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static CastMethod castMethod = CastMethod.Multiphase;

        public SpellMultiphase(UnitEntity caster, SpellParameters parameters) 
            : base(caster, parameters, castMethod)
        {
        }

        public override bool Cast()
        {
            if (!base.Cast())
                return false;

            uint spellDelay = 0;
            for (int i = 0; i < parameters.SpellInfo.Phases.Count; i++)
            {
                int index = i;
                SpellPhaseEntry spellPhase = parameters.SpellInfo.Phases[i];
                spellDelay += spellPhase.PhaseDelay;
                events.EnqueueEvent(new SpellEvent(spellDelay / 1000d, () =>
                {
                    currentPhase = (byte)spellPhase.OrderIndex;
                    effectTriggerCount.Clear();
                    Execute();

                    if (i == parameters.SpellInfo.Phases.Count - 1)
                    {
                        status = SpellStatus.Finishing;
                        log.Trace($"SpellMultiphase {parameters.SpellInfo.Entry.Id} has finished executing.");
                    }
                }));
            }

            status = SpellStatus.Casting;
            log.Trace($"SpellMultiphase {parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && (status == SpellStatus.Casting || status == SpellStatus.Executing);
        }
    }
}
