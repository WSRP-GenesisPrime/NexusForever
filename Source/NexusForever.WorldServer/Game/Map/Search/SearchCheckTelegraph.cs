using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Game.Spell.Static;

namespace NexusForever.WorldServer.Game.Map.Search
{
    public class SearchCheckTelegraph : ISearchCheck
    {
        private readonly Telegraph telegraph;
        private readonly UnitEntity caster;

        public SearchCheckTelegraph(Telegraph telegraph, UnitEntity caster)
        {
            this.telegraph = telegraph;
            this.caster    = caster;
        }

        public bool CheckEntity(GridEntity entity)
        {
            if (telegraph.TelegraphTargetTypeFlags.HasFlag(TelegraphTargetTypeFlags.Self) && entity != caster)
                return false;

            if (telegraph.TelegraphTargetTypeFlags.HasFlag(TelegraphTargetTypeFlags.Other) && entity == caster)
                return false;

            if (entity is not UnitEntity unit)
                return false;

            if (!unit.IsAlive)
                return false;

            bool inTelegraph = telegraph.InsideTelegraph(entity.Position, unit.HitRadius);
            return inTelegraph;
        }
    }
}
