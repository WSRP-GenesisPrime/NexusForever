using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.RBAC.Static;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.None, "Go.", "go")]
    public class GoCommandCategory : CommandCategory
    {
        /// <summary>
        /// Invoke <see cref="CommandCategory"/> with the supplied <see cref="ICommandContext"/> and <see cref="ParameterQueue"/>.
        /// </summary>
        public override CommandResult Invoke(ICommandContext context, ParameterQueue queue)
        {
            CommandResult result = CanInvoke(context); // check permissions.
            if (result != CommandResult.Ok)
                return result;

            string name = queue.Front;
            if(name == null || name.Length <= 0)
            {
                context.SendError("You need to choose a location to go to.");
                return CommandResult.InvalidParameters;
            }

            TeleportCommandCategory.teleportByName(context, name);
            return CommandResult.Ok;
        }
    }
}
