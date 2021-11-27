using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Script
{
    public abstract class SpellScript : Script
    {
        public virtual void OnCast(Spell spell, SpellParameters parameters, UnitEntity caster)
        {
        }

        public virtual void OnExecute(Spell spell, SpellParameters parameters, UnitEntity caster, uint currentPhase, IEnumerable<uint> uniqueTargetIds)
        {
        }
    }
}
