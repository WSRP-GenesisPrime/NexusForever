using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Cinematic.Cinematics;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Loot;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.NorthernWilds
{
    public class Q3486_EmpoweredTower
    {
        const ushort QUEST_EMPOWERED_TOWER = 3486;
        const ushort QUEST_SETTING_UP_CAMP = 3671;

        [Script(3486)]
        public class Q3486_EmpoweredTower_Quest : QuestScript
        {
            public override void OnQuestStateChange(Player player, Quest quest, QuestState newState)
            {
                if (newState == QuestState.Completed)
                {
                    player.CinematicManager.QueueCinematic(new Cinematic_Q3486_EmpoweredTower(player));
                    player.QuestManager.QuestMention(QUEST_SETTING_UP_CAMP);
                }
            }
        }

        [Script(11205)]
        public class Q3486_EmpoweredTower_LoftiteCrystal : CreatureScript
        {
            private VirtualItemEntry rewardItem = GameTableManager.Instance.VirtualItem.GetEntry(206);

            public override void OnCreate(WorldEntity me)
            {
                me.RangeCheck = 5f;
            }

            public override void OnEnterRange(WorldEntity me, WorldEntity activator)
            {
                if (activator is not Player player)
                    return;

                if (player.QuestManager.GetQuestState(QUEST_EMPOWERED_TOWER) != QuestState.Accepted)
                    return;

                GlobalLootManager.Instance.GiveLoot(player.Session, rewardItem, 1u, me.Guid);
                me.ModifyHealth(-me.Health);
            }
        }

    }
}
