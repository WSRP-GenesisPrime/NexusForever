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
    [Command(Permission.Teleport, "A collection of commands to manage teleporting characters.", "teleport", "port", "tele")]
    [CommandTarget(typeof(Player))]
    public class TeleportCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.TeleportCoordinates, "Teleport to the specified coordinates optionally specifying the world.", "coordinates")]
        public void HandleTeleportCoordinates(ICommandContext context,
            [Parameter("X coordinate for target teleport position.")]
            float x,
            [Parameter("Y coordinate for target teleport position.")]
            float y,
            [Parameter("Z coordinate for target teleport position.")]
            float z,
            [Parameter("Optional world id for target teleport position.")]
            ushort? worldId)
        {
            try
            {
                Player target = context.InvokingPlayer;
                if (!target.CanTeleport())
                {
                    context.SendMessage("You have a pending teleport! Please wait to use this command.");
                    return;
                }

                worldId ??= (ushort)target.Map.Entry.Id;

                log.Info($"{target.Name} requesting teleport to coordinates: {worldId.Value} ({x}, {y}, {z}).");

                target.TeleportTo(worldId.Value, x, y, z);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in TeleportCommandCategory.HandleTeleportCoordinates!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.TeleportLocation, "Teleport to the specified world location.", "location")]
        public void HandleTeleportLocation(ICommandContext context,
            [Parameter("World location id for target teleport position.")]
            uint worldLocation2Id)
        {
            try
            {
                WorldLocation2Entry entry = GameTableManager.Instance.WorldLocation2.GetEntry(worldLocation2Id);
                if (entry == null)
                {
                    context.SendMessage($"WorldLocation2 entry not found: {worldLocation2Id}");
                    return;
                }

                Player target = context.InvokingPlayer;
                if (!target.CanTeleport())
                {
                    context.SendMessage("You have a pending teleport! Please wait to use this command.");
                    return;
                }

                var rotation = new Quaternion(entry.Facing0, entry.Facing1, entry.Facing2, entry.Facing3);
                target.Rotation = rotation.ToEulerRadians();
                target.TeleportTo((ushort)entry.WorldId, entry.Position0, entry.Position1, entry.Position2);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in TeleportCommandCategory.HandleTeleportLocation!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        public static void teleportByName(ICommandContext context, string name)
        {
            try
            {
                Player target = context.InvokingPlayer;
                if (!target.CanTeleport())
                {
                    context.SendMessage("You have a pending teleport! Please wait to use this command.");
                    return;
                }

                WorldLocation2Entry zone = SearchManager.Instance.Search<WorldLocation2Entry>(name, context.Language, GetTextIds)
                    .FirstOrDefault();

                log.Info($"{target.Name} requesting teleport to location: {name}.");

                if (zone == null)
                    context.SendMessage($"Unknown zone: {name}");
                else
                {
                    target.TeleportTo((ushort)zone.WorldId, zone.Position0, zone.Position1, zone.Position2);
                    context.SendMessage($"{name}: {zone.WorldId} {zone.Position0} {zone.Position1} {zone.Position2}");
                }
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in TeleportCommandCategory.teleportByName!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.TeleportName, "Teleport to the specified zone name.", "name")]
        public void HandleTeleportName(ICommandContext context,
            [Parameter("Name of the zone for target teleport position.")]
            string name)
        {
            teleportByName(context, name);
        }

        private static IEnumerable<uint> GetTextIds(WorldLocation2Entry entry)
        {
            WorldZoneEntry worldZone = GameTableManager.Instance.WorldZone.GetEntry(entry.WorldZoneId);
            if (worldZone != null && worldZone.LocalizedTextIdName != 0)
                yield return worldZone.LocalizedTextIdName;
            WorldEntry world = GameTableManager.Instance.World.GetEntry(entry.WorldId);
            if (world != null && world.LocalizedTextIdName != 0)
                yield return world.LocalizedTextIdName;
        }
    }
}
