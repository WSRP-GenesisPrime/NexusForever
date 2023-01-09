using NexusForever.WorldServer.Game.Cinematic.Cinematics;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.CrimsonIsle
{
    [Script(5604)]
    public class Q5604_TacticalDemolitions : QuestScript
    {
        static uint QOBJ_EXILE_CANNONS = 8268;
        static uint QOBJ_CINEMATIC_COMPLETE = 15918;

        public override void OnObjectiveUpdate(Player player, Quest quest, QuestObjective objective)
        {
            if (objective.ObjectiveInfo.Id != QOBJ_EXILE_CANNONS)
                return;

            if (objective.IsComplete() && quest.State != QuestState.Achieved)
            {
                player.CinematicManager.QueueCinematic(new Cinematic_Q5604_TacticalDemolitions(player));
                player.QuestManager.ObjectiveUpdate(QOBJ_CINEMATIC_COMPLETE, 1u);
            }
        }
    }
}
