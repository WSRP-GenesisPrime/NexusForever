using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.NorthernWilds
{
    [Script(11194)]
    public class Q3667_TheTower_ControlPanel : CreatureScript
    {
        const ushort QUEST_THE_TOWER = 3667;
        const uint QOBJ_TERMINAL = 4770;

        public override void OnCreate(WorldEntity me)
        {
            me.RangeCheck = 7f;
        }

        public override void OnEnterRange(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            if (player.QuestManager.GetQuestState(QUEST_THE_TOWER) != QuestState.Accepted)
                return;

            player.QuestManager.ObjectiveUpdate(QOBJ_TERMINAL, 1u);
        }
    }
}
