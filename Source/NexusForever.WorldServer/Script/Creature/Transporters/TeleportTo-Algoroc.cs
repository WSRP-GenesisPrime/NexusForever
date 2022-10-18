using NexusForever.Shared;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Script.Creature.Transporters
{
    [Script(27196)] // Ship Controls - Northern Wilds
    public class TeleportTo_TremorRidge : CreatureScript
    {
        const uint WLOC_TREMOR_RIDGE = 9801;

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            WorldLocation2Entry entry = GameTableManager.Instance.WorldLocation2.GetEntry(WLOC_TREMOR_RIDGE);
            if (entry == null)
                return;

            if (!player.CanTeleport())
                return;

            var rotation = new Quaternion(entry.Facing0, entry.Facing1, entry.Facing2, entry.Facing3);
            player.Rotation = rotation.ToEulerRadians();
            player.TeleportTo((ushort)entry.WorldId, entry.Position0, entry.Position1, entry.Position2);
        }
    }
}
