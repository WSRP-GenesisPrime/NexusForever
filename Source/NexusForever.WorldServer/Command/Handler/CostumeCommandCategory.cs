using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Helper;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Costume, "A collection of commands to modify your costume.", "costume")]
    [CommandTarget(typeof(Player))]
    public class CostumeCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.CostumeOverrideId, "Override an item slot with a displayID.", "overrideid")]
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

        [Command(Permission.CostumeOverride, "Override an item slot with an item type and variant.", "override")]
        public void HandleCostumeOverride(ICommandContext context,
            [Parameter("Item slot (your only choice right now is 'weapon').", ParameterFlags.None, typeof(EnumParameterConverter<ItemSlot>))]
            string slotName,
            [Parameter("Override item type.")]
            string itemType,
            [Parameter("Override item variant.")]
            string itemVariant)
        {
            Player p = context.InvokingPlayer;
            if (p.CostumeIndex == 0)
            {
                return;
            }

            ItemSlot slot;
            if ("weapon".Equals(slotName))
            {
                slot = ItemSlot.WeaponPrimary;
            }
            else
            {
                return;
            }

            ushort? displayID = CostumeHelper.GetItemDisplayIdFromType(itemType, itemVariant);
            if (displayID == null)
            {
                context.SendError("A Display ID for the given type and variant could not be found!");
                return;
            }

            Costume costume = p.CostumeManager.GetCostume((byte)p.CostumeIndex);
            costume.setOverride(slot, displayID);
            p.EmitVisualUpdate();
        }

        [Command(Permission.Costume, "Restore an overridden item slot.", "restoreslot")]
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
