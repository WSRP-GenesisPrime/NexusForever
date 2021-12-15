using System.Numerics;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Static;

namespace NexusForever.WorldServer.Game.Map.Search
{
    public class SearchCheckRangeAoeSelect : ISearchCheck
    {
        private readonly Vector3 vector;
        private readonly float radius;
        private readonly GridEntity searcher;
        private readonly SpellTargetMechanicFlags targetMechanicFlags;

        public SearchCheckRangeAoeSelect(GridEntity searcher, float radius, SpellTargetMechanicFlags targetMechanicFlags)
        {
            vector                   = searcher.Position;
            this.radius              = radius;
            this.searcher            = searcher;
            this.targetMechanicFlags = targetMechanicFlags;
        }

        public virtual bool CheckEntity(GridEntity entity)
        {
            if (entity is not UnitEntity unit)
                return false;

            if (!unit.IsAlive)
                return false;

            if (unit is not Player && targetMechanicFlags.HasFlag(SpellTargetMechanicFlags.IsPlayer))
                return false;

            // TODO: Check Angle

            // Check Target Flags
            if (unit.Faction1 == 0 && unit.Faction2 == 0) // Unable to evaluate units with no factions specified, unless this means Neutral?
                return false;

            if (targetMechanicFlags.HasFlag(SpellTargetMechanicFlags.IsEnemy))
            {
                // TODO: handle other things like "Is Immune", "Is Player and PvP Enabled"

                if ((searcher as UnitEntity).GetDispositionTo(unit.Faction1, true) > Reputation.Static.Disposition.Neutral)
                    return false;
            }

            if (targetMechanicFlags.HasFlag(SpellTargetMechanicFlags.IsFriendly))
            {
                if ((searcher as UnitEntity).GetDispositionTo(unit.Faction1, true) < Reputation.Static.Disposition.Neutral)
                    return false;
            } 

            if (Vector3.Distance(vector, entity.Position) > radius)
                return false;

            return true;
        }
    }
}
