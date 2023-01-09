using NexusForever.WorldServer.Game.Cinematic.Cinematics;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Quests.CrimsonIsle
{
    [Script(39962)]
    public class Q5594_LastResistance_ShipControls : CreatureScript
    {
        const ushort WORLD_OLYSSIA = 22;
        private Vector3 LOC_DERADUNE_BLOODFIRE_VILLAGE = new Vector3(-5750.767f, -971.7648f, -623.33295f);
        private Vector3 ROT_DERADUNE_BLOODFIRE_VILLAGE = new Vector3(-1.1721236f, 0f, 0f);

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            if (player.QuestManager.GetQuestState(5594) == QuestState.Accepted)
                player.QuestManager.QuestAchieve(5594);

            if (!player.CanTeleport())
                return;

            player.Rotation = ROT_DERADUNE_BLOODFIRE_VILLAGE;
            player.TeleportTo(WORLD_OLYSSIA, LOC_DERADUNE_BLOODFIRE_VILLAGE.X, LOC_DERADUNE_BLOODFIRE_VILLAGE.Y, LOC_DERADUNE_BLOODFIRE_VILLAGE.Z, reason: TeleportReason.Relocate);
        }
    }

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
