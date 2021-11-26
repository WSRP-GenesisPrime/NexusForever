using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Cinematic.Cinematics.NewCharacter;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Map
{
    [Script(990)]
    public class EverstarGrove : MapScript
    {
        const ushort QUEST_REPORTING_FOR_DUTY = 3480;

        public override void OnAddToMap(Player player)
        {
            if (player.QuestManager.GetQuestState(QUEST_REPORTING_FOR_DUTY) == null)
                player.CinematicManager.QueueCinematic(new Cinematic_NewChar_EverstarGrove(player));
        }
    }
}
