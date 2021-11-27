using NexusForever.WorldServer.Game.Cinematic.Cinematics.NewCharacter;
using NexusForever.WorldServer.Game.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Script.Map
{
    [Script(870)]
    public class CrimsonIsle : MapScript
    {
        static uint Q5596_QOBJ_CRASH_SITE_ZONEID = 1611;
        static ushort QUEST_MIND_THE_MINES_SCRAP_THE_SCRAB = 5593;

        public override void OnAddToMap(Player player)
        {
            if (player.QuestManager.GetQuestState(QUEST_MIND_THE_MINES_SCRAP_THE_SCRAB) == null)
                player.CinematicManager.QueueCinematic(new Cinematic_NewChar_CrimsonIsle(player));
        }

        public override void OnEnterZone(Player player, uint zoneId)
        {
            if (zoneId == Q5596_QOBJ_CRASH_SITE_ZONEID)
                if (player.QuestManager.GetQuestState(5596) == Game.Quest.Static.QuestState.Accepted)
                    player.QuestManager.ObjectiveUpdate(8255, 1u);
        }
    }
}
