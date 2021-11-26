using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Cinematic.Cinematics.NewCharacter;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Map
{
    [Script(1387)]
    public class LevianBay : MapScript
    {
        const ushort QUEST_LIGHTING_THE_WAY = 6780;

        public override void OnAddToMap(Player player)
        {
            if (player.QuestManager.GetQuestState(QUEST_LIGHTING_THE_WAY) == null)
                player.CinematicManager.QueueCinematic(new Cinematic_NewChar_LevianBay(player));
        }
    }
}
