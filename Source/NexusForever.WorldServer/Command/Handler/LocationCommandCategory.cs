using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Shared;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.None, "Utility command for showing the player's exact position in the game world.", "location", "loc")]
    [CommandTarget(typeof(Player))]
    class LocationCommandHandler : CommandCategory
    {
        [Command(Permission.None, "Print your current world position and its teleport coordinates to the chat window.", "print")]
        public void HandleLocationPrint(ICommandContext context)
        {
            float x = context.InvokingPlayer.Position.X;
            float y = context.InvokingPlayer.Position.Y;
            float z = context.InvokingPlayer.Position.Z;
            uint zoneId = context.InvokingPlayer.Zone.Id;
            uint mapId = context.InvokingPlayer.Map.Entry.Id;
            string teleportCommand = $"!teleport coordinates {x} {y} {z} {mapId}";
            context.SendMessage($"Your current location:\n      X:{x}, Y:{y}, Z:{z}\n     World Map ID: {mapId}\n     Zone ID: {zoneId}\n     {teleportCommand}");
        }
    }
}
