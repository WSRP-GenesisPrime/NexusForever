using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Spell
{
    [SpellType(CastMethod.ChanneledField)]
    public partial class SpellChanneledField : Spell, ISpell
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static CastMethod castMethod = CastMethod.ChanneledField;

        public SpellChanneledField(UnitEntity caster, SpellParameters parameters)
            : base(caster, parameters, castMethod)
        {
        }

        public override bool Cast()
        {
            return base.Cast();
        }
    }
}
