using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;

namespace NexusForever.WorldServer.Game.AI
{
    public class PetAI : UnitAI, IUpdate
    {
        public PetAI(UnitEntity unit, uint autoAttackId, double attackTimer)
            : base(unit)
        {
            if (autoAttackId > 0)
            {
                Spell4Entry entry = GameTableManager.Instance.Spell4.GetEntry(autoAttackId);
                if (entry != null)
                {
                    autoAttacks.Clear();
                    autoAttacks.Add(entry.Id);

                    if (MAX_ATTACK_RANGE < entry.TargetMaxRange)
                        MAX_ATTACK_RANGE = entry.TargetMaxRange;
                }

            }
            autoTimer = new UpdateTimer(attackTimer);
            useSpecialAbility = false;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
        }
    }
}
