using System;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Network;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Realm, "A collection of commands to manage the realm.", "realm")]
    public class RealmCommandCategory : CommandCategory
    {
        [Command(Permission.RealmShutdown, "A collection of commands to manage realm shutdown.", "shutdown")]
        public class RealmShutdownCommandCategory : CommandCategory
        {
            [Command(Permission.RealmShutdownStart, "Start a new realm shutdown.", "start")]
            public void HandleRealmShutdownStart(ICommandContext context,
                [Parameter("Time till shutdown. (Format: dd:hh:mm:ss)")]
                TimeSpan span)
            {
                if (ShutdownManager.Instance.IsShutdownPending)
                {
                    context.SendError("Realm already has a pending shutdown!");
                    return;
                }

                ShutdownManager.Instance.StartShutdown(span);
            }

            [Command(Permission.RealmShutdownCancel, "Cancel pending realm shutdown.", "cancel")]
            public void HandleRealmShutdownCancel(ICommandContext context)
            {
                if (!ShutdownManager.Instance.IsShutdownPending)
                {
                    context.SendError("Realm doesn't have a pending shutdown!");
                    return;
                }

                ShutdownManager.Instance.CancelShutdown();
            }
        }

        [Command(Permission.RealmMOTD, "Set the realm's message of the day and announce to the realm.", "motd")]
        public void HandleRealmMotd(ICommandContext context,
            [Parameter("New message of the day for the realm.")]
            string message)
        {
            WorldServer.RealmMotd = message;
            foreach (WorldSession session in NetworkManager<WorldSession>.Instance)
                GlobalChatManager.Instance.SendMessage(session, WorldServer.RealmMotd, "MOTD", ChatChannelType.Realm);
        }

        [Command(Permission.RealmMaxPlayers, "Set the maximum players allowed to connect.", "max")]
        public void HandleRealmMax(ICommandContext context,
            [Parameter("Max players allowed.")]
            uint maxPlayers)
        {
            LoginQueueManager.Instance.SetMaxPlayers(maxPlayers);
        }

        [Command(Permission.RealmOnline, "Displays the users online", "online")]
        public void HandleRealmOnline(ICommandContext context)
        {
            List<WorldSession> allSessions = NetworkManager<WorldSession>.Instance.ToList();

            int index = 0;
            foreach (WorldSession session in allSessions)
            {
                string infoString = "";
                infoString += $"[{index++}] {session.Account?.Email} id:{session.Account?.Id}";

                if (session.Player != null)
                    infoString += $" | {session.Player?.Name}";

                infoString += $" | {session.Uptime:%d}d {session.Uptime:%h}h {session.Uptime:%m}m";

                context.SendMessage(infoString);
            }

            if (allSessions.Count == 0)
                context.SendMessage($"No sessions connected.");
        }

        [Command(Permission.RealmUptime, "Display the current uptime of the server.", "uptime")]
        public void HandleUptimeCheck(ICommandContext context)
        {
            context.SendMessage($"Currently up for {WorldServer.Uptime:%d}d {WorldServer.Uptime:%h}h {WorldServer.Uptime:%m}m {WorldServer.Uptime:%s}s");
        }
    }
}
