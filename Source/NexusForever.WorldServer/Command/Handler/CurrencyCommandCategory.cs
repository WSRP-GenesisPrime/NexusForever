﻿using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Account.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Currency, "A collection of commands to modify account and character currency.", "currency")]
    public class CurrencyCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.CurrencyAccount, "A collection of commands to modify account currency.", "account")]
        public class CurrencyAccountCommandCategory : CommandCategory
        {
            [Command(Permission.CurrencyAccountAdd, "Add currency to account.", "add")]
            [CommandTarget(typeof(Player))]
            public void HandleCurrencyAccountAdd(ICommandContext context,
                [Parameter("Account currency id to grant.", ParameterFlags.None, typeof(EnumParameterConverter<AccountCurrencyType>))]
                AccountCurrencyType currencyId,
                [Parameter("Amount of currency to grant.")]
                uint amount)
            {
                try
                {
                    AccountCurrencyTypeEntry entry = GameTableManager.Instance.AccountCurrencyType.GetEntry((uint)currencyId);
                    if (entry == null || currencyId == AccountCurrencyType.MaxLevelToken) // Disabled Character Token for now due to causing server errors if the player tries to use it. TODO: Fix level 50 creation
                    {
                        context.SendMessage("Invalid currencyId. Please try again.");
                        return;
                    }

                    context.InvokingPlayer.Session.AccountCurrencyManager.CurrencyAddAmount(currencyId, amount);
                }
                catch (Exception e)
                {
                    log.Error($"Exception caught in CurrencyAccountCommandCategory.HandleCurrencyAccountAdd!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                    context.SendError("Oops! An error occurred. Please check your command input and try again.");
                }
            }

            [Command(Permission.CurrencyAccountList, "List all account currency types", "list")]
            public void HandleCurrencyAccountList(ICommandContext context)
            {
                TextTable tt = GameTableManager.Instance.GetTextTable(context.Language);
                foreach (AccountCurrencyTypeEntry entry in GameTableManager.Instance.AccountCurrencyType.Entries)
                    context.SendMessage($"ID {entry.Id}: {tt.GetEntry(entry.LocalizedTextId)}");
            }
        }

        [Command(Permission.CurrencyCharacter, "A collection of commands to modify character currency.", "character")]
        public class CurrencyCharacterCommandCategory : CommandCategory
        {
            [Command(Permission.CurrencyCharacterAdd, "Add currency to character.", "add")]
            [CommandTarget(typeof(Player))]
            public void HandleCurrencyCharacterAdd(ICommandContext context,
                [Parameter("Currency id to grant.", ParameterFlags.None, typeof(EnumParameterConverter<CurrencyType>))]
                CurrencyType currencyId,
                [Parameter("Amount of currency to grant.")]
                uint amount)
            {
                try
                {
                    if (GameTableManager.Instance.CurrencyType.GetEntry((uint)currencyId) == null)
                    {
                        context.SendMessage("Invalid currencyId. Please try again.");
                        return;
                    }

                    context.InvokingPlayer.CurrencyManager.CurrencyAddAmount(currencyId, amount, true);
                }
                catch (Exception e)
                {
                    log.Error($"Exception caught in CurrencyAccountCommandCategory.HandleCurrencyCharacterAdd!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                    context.SendError("Oops! An error occurred. Please check your command input and try again.");
                }
            }

            [Command(Permission.CurrencyCharacterList, "List all currency types.", "list")]
            public void HandleCurrencyCharacterList(ICommandContext context)
            {
                foreach (CurrencyTypeEntry entry in GameTableManager.Instance.CurrencyType.Entries)
                    context.SendMessage($"ID {entry.Id}: {entry.Description}");
            }
        }
    }
}
