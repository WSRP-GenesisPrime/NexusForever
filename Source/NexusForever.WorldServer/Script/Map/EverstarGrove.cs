using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Cinematic.Cinematics.NewCharacter;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Map
{
    [Script(990)]
    public class EverstarGrove : MapScript
    {
        const ushort QUEST_NATURES_UPRISING = 6296;

        public override void OnAddToMap(Player player)
        {
            if (player.QuestManager.GetQuestState(QUEST_NATURES_UPRISING) == null)
                player.CinematicManager.QueueCinematic(new Cinematic_NewChar_EverstarGrove(player));
        }
    }
}
