using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.CrimsonIsle
{
    [Script(10510)]
    public class Q10510_LearningToShop : QuestScript
    {
        static uint QOBJ_PURCHASE_SMART_SHOPPER = 21267u;
        static uint ITEM_TITLE_SMART_SHOPPER = 86245u;
        static ushort TITLE_SMART_SHOPPER = 400;

        public override void OnQuestStateChange(Player player, Quest quest, QuestState newState)
        {
            if (newState < QuestState.Completed)
            {
                if (player.Inventory.HasItem(ITEM_TITLE_SMART_SHOPPER) || player.TitleManager.HasTitle(TITLE_SMART_SHOPPER))
                    player.QuestManager.ObjectiveUpdate(QOBJ_PURCHASE_SMART_SHOPPER, 1u);
            }
        }
    }
}
