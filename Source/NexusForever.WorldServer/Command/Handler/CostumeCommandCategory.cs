using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Helper;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Costume, "A collection of commands to modify your costume.", "costume")]
    [CommandTarget(typeof(Player))]
    public class CostumeCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [Command(Permission.Costume, "Unlock a holo-wardrobe item by item2Id.", "unlockitem")]
        public void HandleUnlockItem(ICommandContext context,
            [Parameter("Item2Id of item to unlock.")]
            uint item2Id)
        {
            string resultMsg = context.InvokingPlayer?.CostumeManager?.UnlockItemByItem2Id(item2Id);
            context.SendMessage(resultMsg);
        }

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
                context.SendError("There is no costume equipped.");
                return;
            }
            Costume costume = p.CostumeManager.GetCostume((byte)p.CostumeIndex);
            if (costume == null)
            {
                context.SendError("The current costume is invalid.");
                return;
            }
            costume.setOverride(slot, displayID);
            p.EmitVisualUpdate();
        }

        [Command(Permission.CostumeOverride, "Override an item slot with an item type and variant.", "override")]
        public void HandleCostumeOverride(ICommandContext context,
            [Parameter("Item slot (your only choice right now is 'weapon').")]
            string slotName,
            [Parameter("Override item type.")]
            string itemType,
            [Parameter("Override item variant.", ParameterFlags.Optional)]
            string itemVariant)
        {
            Player p = context.InvokingPlayer;
            if (p.CostumeIndex == 0)
            {
                context.SendError("There is no costume equipped.");
                return;
            }

            ItemSlot? slot = StringToItemSlot(slotName);
            if (slot == null)
            {
                context.SendError("Invalid item slot: " + slotName);
                return;
            }

            ushort? displayID = CostumeHelper.GetItemDisplayIdFromType((ItemSlot) slot, itemType, itemVariant);
            if (displayID == null)
            {
                context.SendError("A Display ID for the given type and variant could not be found!");
                return;
            }

            HandleCostumeOverrideID(context, (ItemSlot)slot, (ushort)displayID);
        }

        [Command(Permission.CostumeOverride, "Restore an overridden item slot.", "overridelist")]
        public void HandleCostumeList(ICommandContext context,
           [Parameter("Item slot.", ParameterFlags.None)]
            string slotName,
           [Parameter("Category.", ParameterFlags.Optional)]
            string category)
        {
            ItemSlot? slot = StringToItemSlot(slotName);
            if (slot == null)
            {
                context.SendError("Invalid item slot: " + slotName);
                return;
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                string message = $"Available {slotName} types:";
                var list = CostumeHelper.getItemTypeList((ItemSlot) slot);
                foreach(var entry in list)
                {
                    message += $"\n{entry}";
                }
                context.SendMessage(message);
                return;
            }
            { // listing a category. Just a scope thing.
                string message = $"Available {category} items:";
                var list = CostumeHelper.getItemsForType((ItemSlot) slot, category);
                if (list == null)
                {
                    context.SendError("No such category!");
                    return;
                }
                foreach(var entry in list)
                {
                    message += $"\n{entry}";
                }
                context.SendMessage(message);
            }
        }

        [Command(Permission.CostumeOverrideId, "Restore an overridden item slot by itemslot enum.", "restoreslotid")]
        public void HandleCostumeRestoreSlotID(ICommandContext context,
           [Parameter("Item slot.", ParameterFlags.None, typeof(EnumParameterConverter<ItemSlot>))]
            ItemSlot slot)
        {
            Player p = context.InvokingPlayer;
            if (p.CostumeIndex == 0)
            {
                context.SendError("There is no costume equipped.");
                return;
            }
            Costume costume = p.CostumeManager.GetCostume((byte)p.CostumeIndex);
            if (costume == null)
            {
                context.SendError("The current costume is invalid.");
                return;
            }
            costume.setOverride(slot, null);
            p.EmitVisualUpdate();
        }

        [Command(Permission.CostumeOverride, "Restore an overridden item slot.", "restoreslot")]
        public void HandleCostumeRestoreSlot(ICommandContext context,
           [Parameter("Item slot (your only choice right now is 'weapon').")]
            string slotName)
        {
            ItemSlot? slot = StringToItemSlot(slotName);
            if (slot == null)
            {
                context.SendError("Invalid item slot: " + slotName);
                return;
            }

            HandleCostumeRestoreSlotID(context, (ItemSlot)slot);
        }

        public ItemSlot? StringToItemSlot(string slotName)
        {
            if ("weapon".Equals(slotName.ToLower()))
            {
                return ItemSlot.WeaponPrimary;
            }
            if ("bodytype".Equals(slotName.ToLower()))
            {
                return ItemSlot.BodyType;
            }
            else
            {
                return null;
            }
        }
    }
}
