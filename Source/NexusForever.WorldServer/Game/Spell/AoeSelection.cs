using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map.Search;
using NexusForever.WorldServer.Game.Spell.Static;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Spell
{
    public class AoeSelection : IEnumerable<SpellTargetInfo>
    {
        #region IComparers
        class ComparerClosest : IComparer<SpellTargetInfo>
        {
            public int Compare(SpellTargetInfo x, SpellTargetInfo y)
            {
                if (x == null || y == null)
                    return 0;

                if (x.Distance > y.Distance)
                    return 1;

                if (x.Distance < y.Distance)
                    return -1;

                return 0;
            }
        }

        class ComparerFurthest : IComparer<SpellTargetInfo>
        {
            public int Compare(SpellTargetInfo x, SpellTargetInfo y)
            {
                if (x == null || y == null)
                    return 0;

                if (x.Distance < y.Distance)
                    return 1;

                if (x.Distance > y.Distance)
                    return -1;

                return 0;
            }
        }

        class ComparerLowestAbsoluteHealth : IComparer<SpellTargetInfo>
        {
            public int Compare(SpellTargetInfo x, SpellTargetInfo y)
            {
                if (x == null || y == null)
                    return 0;

                if (x.Entity.Health > y.Entity.Health)
                    return 1;

                if (x.Entity.Health < y.Entity.Health)
                    return -1;

                return 0;
            }
        }

        class ComparerMissingMostHealth : IComparer<SpellTargetInfo>
        {
            public int Compare(SpellTargetInfo x, SpellTargetInfo y)
            {
                if (x == null || y == null)
                    return 0;

                if (x.Entity.MaxHealth - x.Entity.Health < y.Entity.MaxHealth - y.Entity.Health)
                    return 1;

                if (x.Entity.MaxHealth - x.Entity.Health > y.Entity.MaxHealth - y.Entity.Health)
                    return -1;

                return 0;
            }
        }

        #endregion

        private UnitEntity caster;
        private Vector3 initialPosition;
        private SpellInfo info;
        private SpellParameters parameters;

        private float maxRange => info.Entry.TargetMaxRange;
        private SpellTargetMechanicType targetType => (SpellTargetMechanicType)info.BaseInfo.TargetMechanics.TargetType;
        private SpellTargetMechanicFlags targetFlags => (SpellTargetMechanicFlags)info.BaseInfo.TargetMechanics.Flags;
        private AoeSelectionType selectionType => info.AoeTargetConstraints != null ? (AoeSelectionType)info.AoeTargetConstraints.TargetSelection : AoeSelectionType.None;
        
        private List<SpellTargetInfo> validatedTargets = new();

        public AoeSelection(UnitEntity caster, SpellParameters parameters)
        {
            this.caster     = caster;
            this.parameters = parameters;
            this.info       = parameters.SpellInfo;
            initialPosition = caster.Position;

            if (parameters.PositionalUnitId > 0)
                initialPosition = caster.GetVisible<WorldEntity>(parameters.PositionalUnitId)?.Position ?? caster.Position;

            SelectTargets();
            OrderForSelectionType();
        }

        private void SelectTargets()
        {
            if (targetType == SpellTargetMechanicType.Self || targetType == SpellTargetMechanicType.PrimaryTarget)
                return;

            // TODO: Use Target Type to calculate positions

            caster.Map.Search(initialPosition, maxRange, new SearchCheckRangeAoeSelect(caster, initialPosition, maxRange, targetFlags), out List<GridEntity> selectedTargets);

            foreach (var target in selectedTargets)
                validatedTargets.Add(new SpellTargetInfo(SpellEffectTargetFlags.Telegraph, target as UnitEntity, Vector3.Distance(caster.Position, target.Position)));
        }

        private void OrderForSelectionType()
        {
            switch (selectionType)
            {
                case AoeSelectionType.Closest:
                    validatedTargets.Sort(new ComparerClosest());
                    break;
                case AoeSelectionType.Furthest:
                    validatedTargets.Sort(new ComparerFurthest());
                    break;
                case AoeSelectionType.Random:
                    validatedTargets = validatedTargets.OrderBy(x => Random.Shared.Next()).ToList();
                    break;
                case AoeSelectionType.LowestAbsoluteHealth:
                    validatedTargets.Sort(new ComparerLowestAbsoluteHealth());
                    break;
                case AoeSelectionType.MissingMostHealth:
                    validatedTargets.Sort(new ComparerMissingMostHealth());
                    break;
                case AoeSelectionType.None:
                default:
                    // Do nothing. Leave it ordered as it was evaluated.
                    break;
            }
        }

        public IEnumerator<SpellTargetInfo> GetEnumerator()
        {
            return validatedTargets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
