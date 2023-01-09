using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Cinematic.Cinematics;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Loot;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.NorthernWilds
{
    [Script(3673)]
    public class Q3673_ContactWithThayd : QuestScript
    {
        public override void OnQuestStateChange(Player player, Quest quest, QuestState newState)
        {
            if (newState == QuestState.Achieved)
                player.CinematicManager.QueueCinematic(new Cinematic_Q3673_ContactWithThayd(player));
        }
    }
}
