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
        [Command(Permission.CostumeOverrideId, "Override an item slot with a displayID.", "overrideid")]
        public void HandleCostumeOverrideID(ICommandContext context,
            [Parameter("Item slot.", ParameterFlags.None, typeof(EnumParameterConverter<ItemSlot>))]
            ItemSlot slot,
            [Parameter("DisplayID to override with.")]
            ushort displayID)
        {
            try
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
            catch (Exception e)
            {
                log.Error($"Exception caught in CostumeCommandCategory.HandleCostumeOverrideID!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.CostumeOverride, "Override an item slot with an item type and variant.", "override")]
        public void HandleCostumeOverride(ICommandContext context,
            [Parameter("Item slot (your only choice right now is 'weapon').")]
            string slotName,
            [Parameter("Override item type.")]
            string itemType,
            [Parameter("Override item variant.")]
            string itemVariant)
        {
            try
            {
                Player p = context.InvokingPlayer;
                if (p.CostumeIndex == 0)
                {
                    context.SendError("There is no costume equipped.");
                    return;
                }

                ItemSlot slot;
                if ("weapon".Equals(slotName.ToLower()))
                {
                    slot = ItemSlot.WeaponPrimary;
                }
                else
                {
                    context.SendError("Invalid item slot: " + slotName);
                    return;
                }

                ushort? displayID = CostumeHelper.GetItemDisplayIdFromType(itemType, itemVariant);
                if (displayID == null)
                {
                    context.SendError("A Display ID for the given type and variant could not be found!");
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
            catch (Exception e)
            {
                log.Error($"Exception caught in CostumeCommandCategory.HandleCostumeOverride!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.CostumeOverride, "Restore an overridden item slot.", "overridelist")]
        public void HandleCostumeList(ICommandContext context,
           [Parameter("Item slot.", ParameterFlags.None)]
            string slotName,
           [Parameter("Category.", ParameterFlags.Optional)]
            string category)
        {
            if (!"weapon".Equals(slotName.ToLower()))
            {
                context.SendError("Invalid item slot: " + slotName);
                return;
            }
            if (string.IsNullOrWhiteSpace(category))
            {
                string message = $"Available {slotName} types:";
                var list = CostumeHelper.getItemTypeList();
                foreach(var entry in list)
                {
                    message += $"\n{entry}";
                }
                context.SendMessage(message);
                return;
            }
            { // listing a category. Just a scope thing.
                string message = $"Available {category} items:";
                var list = CostumeHelper.getItemsForType(category);
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

        [Command(Permission.CostumeOverride, "Restore an overridden item slot.", "restoreslot")]
        public void HandleCostumeRestoreSlot(ICommandContext context,
           [Parameter("Item slot.", ParameterFlags.None, typeof(EnumParameterConverter<ItemSlot>))]
            ItemSlot slot)
        {
            try {
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
            catch (Exception e)
            {
                log.Error($"Exception caught in CostumeCommandCategory.HandleCostumeRestoreSlot!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }
    }
}
