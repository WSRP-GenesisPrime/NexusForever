﻿using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class VehicleHandler
    {
        [MessageHandler(GameMessageOpcode.ClientVehicleDisembark)]
        public static void HandleVehicleDisembark(WorldSession session, ClientVehicleDisembark disembark)
        {
            // If player is mounted and tries to summon a different mount, the client sends this packet twice.
            // Ignore Disembark request if no vehicle.
            if (session.Player.VehicleGuid == 0u)
                return;

            session.Player.Dismount();
        }
    }
}
