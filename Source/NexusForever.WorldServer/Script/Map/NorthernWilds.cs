using NexusForever.Shared.GameTable;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Cinematic.Cinematics.NewCharacter;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Map
{
    [Script(426)]
    public class NorthernWilds : MapScript
    {
        const uint Q3486_EMPOWERED_TOWER_ZONEID = 729;
        const ushort QUEST_EMPOWERED_TOWER = 3486;
        const uint QOBJ_ARRIVED_AT_TOWER = 4987;
        const uint STORYPANEL_ARRIVED_AT_TOWER = 1575;

        const ushort QUEST_REPORTING_FOR_DUTY = 3480;

        public override void OnAddToMap(Player player)
        {
            if (player.QuestManager.GetQuestState(QUEST_REPORTING_FOR_DUTY) == null)
                player.CinematicManager.QueueCinematic(new Cinematic_NewChar_NorthernWilds(player));
        }

        public override void OnEnterZone(Player player, uint zoneId)
        {
            if (zoneId == Q3486_EMPOWERED_TOWER_ZONEID)
            {
                if (player.QuestManager.GetQuestState(QUEST_EMPOWERED_TOWER) == QuestState.Accepted)
                {
                    StoryBuilder.Instance.SendStoryPanel(GameTableManager.Instance.StoryPanel.GetEntry(STORYPANEL_ARRIVED_AT_TOWER), player);
                    player.QuestManager.ObjectiveUpdate(4987, 1u);
                }
            }
        }
    }
}
