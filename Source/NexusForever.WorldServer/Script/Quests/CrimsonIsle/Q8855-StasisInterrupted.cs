using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.CrimsonIsle
{
    public class Q8855_StasisInterrupted
    {
        static ushort QUEST_ID = 8855;

        [Script(47687)]
        [Script(47688)]
        public class Q8855_DominionSoldiers : CreatureScript
        {
            public override void OnCreate(WorldEntity me)
            {
                me.RangeCheck = 5f;
            }

            public override void OnEnterRange(WorldEntity me, WorldEntity activator)
            {
                if (activator is not Player player)
                    return;

                if (player.QuestManager.GetQuestState(QUEST_ID) == QuestState.Accepted)
                {
                    player.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateEntity, me.CreatureId, 1u);
                    me.ModifyHealth(-me.Health);
                }
            }
        }
    }
}
