using NexusForever.WorldServer.Game.Cinematic.Cinematics;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script.Quests.CrimsonIsle
{
    [Script(31792)]
    public class Q5594_LastResistance_Warbot : CreatureScript
    {
        const uint QOBJ_WARBOT_KILL = 8249;
        const ushort ACH_WARBOT = 1730;

        public override void OnDeathRewardGrant(WorldEntity me, WorldEntity killer)
        {
            if (killer is not Player player)
                return;

            player.AchievementManager.GrantAchievement(ACH_WARBOT);
            player.QuestManager.ObjectiveUpdate(QOBJ_WARBOT_KILL, 1u);
        }
    }
}
