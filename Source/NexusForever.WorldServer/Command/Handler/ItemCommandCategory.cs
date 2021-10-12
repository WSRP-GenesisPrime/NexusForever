using System;
using System.Collections.Generic;
using System.Linq;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Social.Static;
using NLog;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Item, "A collection of commands to manage items for a character.", "item")]
    [CommandTarget(typeof(Player))]
    public class ItemCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.ItemAdd, "Add an item to inventory, optionally specifying quantity and charges.", "add")]
        public void HandleItemAdd(ICommandContext context,
            [Parameter("Item id to create.")]
            uint itemId,
            [Parameter("Quantity of item to create.")]
            uint? quantity,
            [Parameter("Amount of charges to add to the item.")]
            uint? charges)
        {
            try
            {
                quantity ??= 1u;
                charges ??= 1u;
                log.Info($"{context.InvokingPlayer.Name} requesting to add item ID {itemId} (x{quantity}, {charges} charges).");
                context.InvokingPlayer.Inventory.ItemCreate(InventoryLocation.Inventory, itemId, quantity.Value, ItemUpdateReason.Cheat, charges.Value);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in ItemCommandCategory.HandleItemAdd!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.ItemLookup, "Lookup an item by partial name.", "lookup")]
        public void HandleItemLookup(ICommandContext context,
            [Parameter("Item name to lookup.")]
            string name,
            [Parameter("Maximum amount of results to return.")]
            int? maxResults)
        {
            try
            {
                List<Item2Entry> searchResults = SearchManager.Instance
                    .Search<Item2Entry>(name, context.Language, e => e.LocalizedTextIdName, true)
                    .Take(maxResults ?? 25)
                    .ToList();

                if (searchResults.Count == 0)
                {
                    context.SendMessage($"Item lookup results was 0 entries for '{name}'.");
                    return;
                }

                context.SendMessage($"Item lookup results for '{name}' ({searchResults.Count}):");

                var target = context.InvokingPlayer;
                foreach (Item2Entry itemEntry in searchResults)
                {
                    var builder = new ChatMessageBuilder
                    {
                        Type = ChatChannelType.System,
                        Text = $"({itemEntry.Id}) "
                    };
                    builder.AppendItem(itemEntry.Id);
                    target.Session.EnqueueMessageEncrypted(builder.Build());
                }
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in ItemCommandCategory.HandleItemLookup!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }
    }
}
