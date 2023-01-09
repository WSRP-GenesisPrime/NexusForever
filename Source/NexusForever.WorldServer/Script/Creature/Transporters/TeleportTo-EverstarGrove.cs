using NexusForever.WorldServer.Game.Entity;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.Transporters
{
    [Script(70172)] // Transport - Northern Wilds
    public class TeleportTo_GreenleafGlade : CreatureScript
    {
        const ushort WLOC_EVERSTAR_GROVE = 990;
        private Vector3 LOC_GREENLEAF_GLADE = new Vector3(-771.823f, -904.285f, -2269.56f);
        private Vector3 ROT_GREENLEAF_GLADE = new Vector3(-1.1214001f, 0f, 0f);

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            if (!player.CanTeleport())
                return;

            player.Rotation = ROT_GREENLEAF_GLADE;
            player.TeleportTo(WLOC_EVERSTAR_GROVE, LOC_GREENLEAF_GLADE.X, LOC_GREENLEAF_GLADE.Y, LOC_GREENLEAF_GLADE.Z);
        }
    }
}
