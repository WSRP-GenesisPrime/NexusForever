using NexusForever.Shared.Network;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Chron, "Send a message to another player's datachron channel.", "chron")]
    public class ChronCommandCategory : CommandCategory
    {
        /// <summary>
        /// Invoke <see cref="CommandCategory"/> with the supplied <see cref="ICommandContext"/> and <see cref="ParameterQueue"/>.
        /// </summary>
        public override CommandResult Invoke(ICommandContext context, ParameterQueue queue)
        {
            CommandResult result = CanInvoke(context); // check permissions.
            if (result != CommandResult.Ok)
                return result;

            ChronCommandParameterConverter con = new ChronCommandParameterConverter();
            con.Convert(context, queue);

            Player player = context.InvokingPlayer;

            Player targetPlayer = null;

            if (con.PlayerName != null && con.PlayerName.Length > 0)
            {
                targetPlayer = NetworkManager<WorldSession>.Instance.GetSession(s => s.Player?.Name == con.PlayerName)?.Player;
            }
            else
            {
                context.SendError($"Player {con.PlayerName} not found.");
                return CommandResult.InvalidParameters;
            }

            string echoText = con.TextMode ? "[TXT]> " + con.Message : con.Message;
            string targetText = con.TextMode ? "[TXT]< " + con.Message : con.Message;

            ChatMessageBuilder cmb = new ChatMessageBuilder()
            {
                Guid = targetPlayer.Guid,
                Type = Game.Social.Static.ChatChannelType.Datachron,
                FromName = "to " + targetPlayer.Name,
                Self = true,
                Text = echoText
            };
            player.Session.EnqueueMessageEncrypted(cmb.Build());

            cmb = new ChatMessageBuilder()
            {
                Guid = targetPlayer.Guid,
                Type = Game.Social.Static.ChatChannelType.Datachron,
                FromName = "from " + player.Name,
                Self = false,
                Text = targetText
            };
            targetPlayer.Session.EnqueueMessageEncrypted(cmb.Build());

            return CommandResult.Ok;
        }
    }
}
