using System;
using System.Collections.Generic;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map.Search;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Door, "A collection of commands to interact with door entities.", "door")]
    [CommandTarget(typeof(Player))]
    public class DoorCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.DoorOpen, "Open all doors within a specified range.", "open")]
        public void HandleDoorOpen(ICommandContext context,
            [Parameter("Distance to search for doors to open.")]
            float? searchRange)
        {
            try
            {
                searchRange ??= 10f;

                Player player = context.InvokingPlayer;
                player.Map.Search(
                    player.Position,
                    searchRange.Value,
                    new SearchCheckRangeDoorOnly(player.Position, searchRange.Value, player),
                    out List<GridEntity> intersectedEntities
                );

                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (Door door in intersectedEntities)
                {
                    context.SendMessage($"Trying to open door {door.Guid}");
                    door.OpenDoor();
                }
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in DoorCommandCategory.HandleDoorOpen!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.DoorClose, "Close all doors within a specified range.", "close")]
        public void HandleDoorClose(ICommandContext context,
            [Parameter("Distance to search for doors to close.")]
            float? searchRange)
        {
            try
            {
                searchRange ??= 10f;

                Player player = context.InvokingPlayer;
                player.Map.Search(
                    player.Position,
                    searchRange.Value,
                    new SearchCheckRangeDoorOnly(player.Position, searchRange.Value, player),
                    out List<GridEntity> intersectedEntities
                );

                // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                foreach (Door door in intersectedEntities)
                {
                    context.SendMessage($"Trying to close door {door.Guid}");
                    door.CloseDoor();
                }
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in DoorCommandCategory.HandleDoorClose!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }
    }
}
