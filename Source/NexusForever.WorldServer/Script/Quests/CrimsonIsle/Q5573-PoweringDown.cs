using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.CrimsonIsle
{
    [Script(5573)]
    public class Q5573_PoweringDown : QuestScript
    {
        static uint QOBJ_POWER_REGULATORS = 8229;
        static uint QOBJ_CINEMATIC_COMPLETE = 12870;

        public override void OnObjectiveUpdate(Player player, Quest quest, QuestObjective objective)
        {
            if (objective.ObjectiveInfo.Id != QOBJ_POWER_REGULATORS)
                return;

            if (objective.IsComplete() && quest.State != QuestState.Achieved)
            {
                // Play Cinematic
                player.QuestManager.ObjectiveUpdate(QOBJ_CINEMATIC_COMPLETE, 1u);
            }
        }
    }
}
