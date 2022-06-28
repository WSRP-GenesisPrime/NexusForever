using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Map.Static;
using NexusForever.WorldServer.Game.RBAC.Static;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Map, "A collection of commands to manage maps.", "map")]
    [CommandTarget(typeof(Player))]
    public class MapCommandCategory : CommandCategory
    {
        [Command(Permission.MapUnload, "Unload current map instance.", "unload")]
        public void HandleMapUnload(ICommandContext context)
        {
            Player player = context.GetTargetOrInvoker<Player>();
            if (player.Map is not MapInstance instance)
            {
                context.SendError("Current map is not an instance!");
                return;
            }

            instance.Unload();
        }

        [Command(Permission.MapUnload, "Unload map instance by name.", "unloadbyname")]
        public void HandleMapUnloadByName(ICommandContext context,
            [Parameter("Name.")]
            string name,
            [Parameter("Last name.", Static.ParameterFlags.Optional)]
            string name2)
        {
            if (!string.IsNullOrWhiteSpace(name2))
            {
                name = name + " " + name2;
            }

            Residence res = GlobalResidenceManager.Instance.GetResidenceByOwner(name);

            if (res != null)
            {
                if (res.Map != null)
                {
                    res.Map.Unload();
                    return;
                }
                context.SendError("Residence map is not loaded.");
            }

            context.SendError($"Residence not found: {name}.");
        }

        [Command(Permission.MapPlayerRemove, "Remove player from current map instance.", "remove")]
        public void HandleMapPlayerRemove(ICommandContext context,
            [Parameter("Removal reason.", converter: typeof(EnumParameterConverter<WorldRemovalReason>))]
            WorldRemovalReason removalReason)
        {
            Player player = context.GetTargetOrInvoker<Player>();
            if (player.Map is not MapInstance instance)
            {
                context.SendError("Current map is not an instance!");
                return;
            }

            instance.EnqueuePendingRemoval(player, removalReason);
        }

        [Command(Permission.MapPlayerRemoveCancel, "Cancel removal of player from current map instance.", "cancel")]
        public void HandleMapPlayerRemoveCancel(ICommandContext context)
        {
            Player player = context.GetTargetOrInvoker<Player>();
            if (player.Map is not MapInstance instance)
            {
                context.SendError("Current map is not an instance!");
                return;
            }

            instance.CancelPendingRemoval(player);
        }
    }
}
