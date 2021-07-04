using NexusForever.Shared.GameTable;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Entitlement, "A collection of commands to manage account and character entitlements.", "entitlement")]
    [CommandTarget(typeof(Player))]
    public class EntitlementCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.EntitlementAccount, "A collection of commands to manage account entitlements", "account")]
        public class EntitlementCommandAccountCategory : CommandCategory
        {
            [Command(Permission.EntitlementAccountAdd, "Create or update an account entitlement.", "add")]
            public void HandleEntitlementCommandAccountAdd(ICommandContext context,
                [Parameter("Entitlement type to modify.", ParameterFlags.None, typeof(EnumParameterConverter<EntitlementType>))]
                EntitlementType entitlementType,
                [Parameter("Value to modify the entitlement.")]
                int value)
            {
                try
                {
                    if (GameTableManager.Instance.Entitlement.GetEntry((ulong)entitlementType) == null)
                    {
                        context.SendMessage($"{entitlementType} isn't a valid entitlement id!");
                        return;
                    }
                    log.Info($"{context.InvokingPlayer.Name} ({context.InvokingPlayer.Session.Account.Email}) requesting account entitlement ID {entitlementType} (value: {value}).");
                    context.InvokingPlayer.Session.EntitlementManager.SetAccountEntitlement(entitlementType, value);
                }
                catch (Exception e)
                {
                    log.Error($"Exception caught in EntitlementCommandAccountCategory.HandleEntitlementCommandAccountAdd!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                    context.SendError("Oops! An error occurred. Please check your command input and try again.");
                }
            }

            [Command(Permission.EntitlementAccountList, "List all entitlements for character.", "list")]
            public void HandleEntitlementCommandAccountList(ICommandContext context)
            {
                Player player = context.InvokingPlayer;
                context.SendMessage($"Entitlements for account {player.Session.Account.Id}:");
                foreach (AccountEntitlement entitlement in player.Session.EntitlementManager.GetAccountEntitlements()) 
                    context.SendMessage($"Entitlement: {entitlement.Type}, Value: {entitlement.Amount}");
            }
        }

        [Command(Permission.EntitlementCharacter, "A collection of commands to manage character entitlements", "character")]
        public class EntitlementCommandCharacterCategory : CommandCategory
        {
            [Command(Permission.EntitlementCharacterAdd, "Create or update a character entitlement.", "add")]
            public void HandleEntitlementCommandCharacterAdd(ICommandContext context,
                [Parameter("Entitlement type to modify.", ParameterFlags.None, typeof(EnumParameterConverter<EntitlementType>))]
                EntitlementType entitlementType,
                [Parameter("Value to modify the entitlement.")]
                int value)
            {
                try
                {
                    if (GameTableManager.Instance.Entitlement.GetEntry((ulong)entitlementType) == null)
                    {
                        context.SendMessage($"{entitlementType} isn't a valid entitlement id!");
                        return;
                    }
                    log.Info($"{context.InvokingPlayer.Name} requesting character entitlement ID {entitlementType} (value: {value}).");
                    context.InvokingPlayer.Session.EntitlementManager.SetCharacterEntitlement(entitlementType, value);
                }
                catch (Exception e)
                {
                    log.Error($"Exception caught in EntitlementCommandCharacterCategory.HandleEntitlementCommandCharacterAdd!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                    context.SendError("Oops! An error occurred. Please check your command input and try again.");
                }
            }

            [Command(Permission.EntitlementCharacterList, "List all entitlements for account.", "list")]
            public void HandleEntitlementCommandCharacterList(ICommandContext context)
            {
                Player player = context.InvokingPlayer;
                context.SendMessage($"Entitlements for character {player.Session.Player.CharacterId}:");
                foreach (CharacterEntitlement entitlement in player.Session.EntitlementManager.GetCharacterEntitlements())
                    context.SendMessage($"Entitlement: {entitlement.Type}, Value: {entitlement.Amount}");
            }
        }
    }
}
