using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.Deradune
{
    [Script(45366)]
    public class ShipControls_CrimsonIsle : CreatureScript
    {
        const ushort WORLD_CRIMSON_ISLE = 870;
        private Vector3 LOC_MONDOS_BEACHHEAD = new Vector3(-8267.476f, -995.66176f, -239.02145f);
        private Vector3 ROT_MONDOS_BEACHHEAD = new Vector3(-1.8304919f, 0f, 0f);

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            if (!player.CanTeleport())
                return;

            player.Rotation = ROT_MONDOS_BEACHHEAD;
            player.TeleportTo(WORLD_CRIMSON_ISLE, LOC_MONDOS_BEACHHEAD.X, LOC_MONDOS_BEACHHEAD.Y, LOC_MONDOS_BEACHHEAD.Z, reason: TeleportReason.Relocate);
        }
    }
}
