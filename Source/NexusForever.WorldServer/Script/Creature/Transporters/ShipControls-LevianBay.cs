using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.Transporters
{
    [Script(30352)] // Transport - Ellevar
    [Script(70173)] // Transport - Crimson Isle
    public class ShipControls_LevianBay : CreatureScript
    {
        const ushort WORLD_CRIMSON_ISLE = 1387;
        private Vector3 LOC_STORMCALLER_LANDING = new Vector3(-3835.34f, -980.217f, -6050.52f);
        private Vector3 ROT_STORMCALLER_LANDING = new Vector3(-0.45682f, 0f, 0f);

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            if (!player.CanTeleport())
                return;

            player.Rotation = ROT_STORMCALLER_LANDING;
            player.TeleportTo(WORLD_CRIMSON_ISLE, LOC_STORMCALLER_LANDING.X, LOC_STORMCALLER_LANDING.Y, LOC_STORMCALLER_LANDING.Z, reason: TeleportReason.Relocate);
        }
    }
}
