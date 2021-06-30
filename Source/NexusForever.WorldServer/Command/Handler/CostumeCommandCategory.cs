using System.Text;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.None, "A collection of commands to modify your costume.", "costume")]
    [CommandTarget(typeof(Player))]
    public class CostumeCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.GMFlag, "Override an item slot with a displayID.", "overrideid")]
        public void HandleCostumeOverrideID(ICommandContext context,
            [Parameter("Item slot.", ParameterFlags.None, typeof(EnumParameterConverter<ItemSlot>))]
            ItemSlot slot,
            [Parameter("DisplayID to override with.")]
            ushort displayID)
        {
            Player p = context.InvokingPlayer;
            if (p.CostumeIndex == 0)
            {
                return;
            }
            Costume costume = p.CostumeManager.GetCostume((byte)p.CostumeIndex);
            costume.setOverride(slot, displayID);
            p.EmitVisualUpdate();
        }

        [Command(Permission.None, "Restore an overridden item slot.", "restoreslot")]
        public void HandleCostumeRestoreSlot(ICommandContext context,
           [Parameter("Item slot.", ParameterFlags.None, typeof(EnumParameterConverter<ItemSlot>))]
            ItemSlot slot)
        {
            Player p = context.InvokingPlayer;
            if (p.CostumeIndex == 0)
            {
                return;
            }
            Costume costume = p.CostumeManager.GetCostume((byte)p.CostumeIndex);
            costume.setOverride(slot, null);
            p.EmitVisualUpdate();
        }
    }
}
