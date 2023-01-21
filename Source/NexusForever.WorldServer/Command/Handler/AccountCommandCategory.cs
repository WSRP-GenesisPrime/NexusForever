﻿using NexusForever.Database.Auth.Model;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.Cryptography;
using NexusForever.Shared.Database;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using static NexusForever.WorldServer.Network.Message.Model.ServerMailAvailable;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Account, "A collection of commands to modify game accounts.", "acc", "account")]
    public class AccountCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.AccountCreate, "Create a new account.", "create")]
        public void HandleAccountCreate(ICommandContext context,
            [Parameter("Email address for the new account", converter: typeof(StringLowerParameterConverter))]
            string email,
            [Parameter("Password for the new account")]
            string password,
            [Parameter("Role", ParameterFlags.Optional)]
            uint? role = null,
            [Parameter("Link", ParameterFlags.Optional)]
            string link = null)
        {
           
            if (DatabaseManager.Instance.AuthDatabase.AccountExists(email))
            {
                context.SendMessage("Account with that username already exists. Please try another.");
                return;
            }
                
            role ??= (ConfigurationManager<WorldServerConfiguration>.Instance.Config.DefaultRole ?? (uint)Role.Player);
                
            (string salt, string verifier) = PasswordProvider.GenerateSaltAndVerifier(email, password);
            DatabaseManager.Instance.AuthDatabase.CreateAccount(email, salt, verifier, (uint)role, link);

            if (context.InvokingPlayer != null)
            {
                log.Info($"Account {email} created successfully by {context.InvokingPlayer.Name} ({context.InvokingPlayer.Session.Account.Email}).");
            }
            else
            {
                log.Info($"Account {email} created successfully.");
            }

            context.SendMessage($"Account {email} created successfully");
        }

        [Command(Permission.AccountCreate, "Generate a new account link.", "linkgen")]
        public void HandleAccountLinkGenerate(ICommandContext context,
            [Parameter("Comment to be stored with the account link.")]
            string comment)
        {
            string newLink = GetUniqueKey(8);

            if (context.InvokingPlayer != null)
                DatabaseManager.Instance.AuthDatabase.CreateAccountLink(newLink, DateTime.Now, comment, context.InvokingPlayer.Session.Account.Email);
            else
                DatabaseManager.Instance.AuthDatabase.CreateAccountLink(newLink, DateTime.Now, comment);
                
            context.SendMessage($"Account link {newLink} created successfully");
        }

        internal static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        public static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        [Command(Permission.AccountCreate, "Add an account to an existing account link.", "link")]
        public void HandleAccountLink(ICommandContext context,
            [Parameter("Username of the account to be linked.", converter: typeof(StringLowerParameterConverter))]
            string name,
            [Parameter("The account link.")]
            string link)
        {
            if (!DatabaseManager.Instance.AuthDatabase.AccountExists(name))
            {
                context.SendError("That account does not exist.");
                return;
            }
            if (DatabaseManager.Instance.AuthDatabase.AccountIsLinked(name))
            {
                context.SendError("That account is already linked.");
                return;
            }
            if (!DatabaseManager.Instance.AuthDatabase.AccountLinkExists(link))
            {
                context.SendError("That account link does not exist.");
                return;
            }

            DatabaseManager.Instance.AuthDatabase.LinkAccount(name, link);
            context.SendMessage($"Account {name} linked successfully.");
        }
        /*
         * Needs work- keeps generating new links even if one of the accounts is already on a link
         * 
        [Command(Permission.AccountCreate, "Link two accounts. If neither account has a link associated with it, a new link will be created.", "linkto")]
        public void HandleAccountLinkTo(ICommandContext context,
            [Parameter("Username of the account to be linked.", converter: typeof(StringLowerParameterConverter))]
            string firstAccount,
            [Parameter("Username of the account being linked to.", converter: typeof(StringLowerParameterConverter))]
            string secondAccount)
        {
            if (!DatabaseManager.Instance.AuthDatabase.AccountExists(firstAccount) || !DatabaseManager.Instance.AuthDatabase.AccountExists(secondAccount))
            {
                context.SendError("One or both of those accounts do not exist.");
                return;
            }
            if (DatabaseManager.Instance.AuthDatabase.AccountIsLinked(firstAccount) && DatabaseManager.Instance.AuthDatabase.AccountIsLinked(secondAccount))
            {
                context.SendError("Both of those accounts have existing links.");
                return;
            }

            string link = DatabaseManager.Instance.AuthDatabase.LinkAccounts(firstAccount, secondAccount);
            context.SendMessage($"{firstAccount} was successfully linked to {secondAccount}. Link ID: {link}");
        }
        */
        [Command(Permission.Account, "A collection of commands to pull account data from your other linked accounts.", "sync")]
        public class AccountSyncCommandCategory : CommandCategory
        {
            [Command(Permission.Account, "Pull all account unlocks from your linked accounts.", "all")]
            public void HandleAccountSyncAll(ICommandContext context)
            {
                AccountModel account = context.InvokingPlayer.Session.Account;
                if (!DatabaseManager.Instance.AuthDatabase.AccountIsLinked(account.Email))
                {
                    context.SendError("This account is unlinked.");
                    return;
                }
                context.SendMessage($"Full account sync initiated. Please wait.");

                //Sync costume item unlocks
                HandleAccountSyncCostumes(context);

                //TODO: Sync entitlements
                //TODO: Sync currencies
                //TODO: Sync achievements
                //TODO: Sync keybindings
                
                context.SendMessage($"Full account sync complete.");
            }

            [Command(Permission.Account, "Pull costume item unlocks from your linked accounts.", "costume")]
            public void HandleAccountSyncCostumes(ICommandContext context)
            {
                AccountModel account = context.InvokingPlayer.Session.Account;
                if (!DatabaseManager.Instance.AuthDatabase.AccountIsLinked(account.Email))
                {
                    context.SendError("This account is unlinked.");
                    return;
                }
                context.SendMessage($"Beginning account costume item unlock sync.");
                List<uint> linkedAccountsCostumeUnlockItemIds = DatabaseManager.Instance.AuthDatabase.GetAccountCostumeUnlockItemIdsForSync(account);
                if (linkedAccountsCostumeUnlockItemIds != null)
                {
                    context.SendMessage($"Syncing {linkedAccountsCostumeUnlockItemIds.Count} costume unlocks to this account...");
                    foreach (uint item2Id in linkedAccountsCostumeUnlockItemIds)
                    {
                        context.InvokingPlayer?.CostumeManager?.UnlockItemByItem2Id(item2Id);
                    }
                    context.SendMessage($"{linkedAccountsCostumeUnlockItemIds.Count} costume unlocks have been synced.");
                }
            }
        }

        [Command(Permission.Account, "Change the password of your account.", "changemypass")]
        [CommandTarget(typeof(Player))]
        public void HandleAccountChangeMyPass(ICommandContext context,
            [Parameter("New password")]
            string password)
        {
            try
            {
                Player target = context.InvokingPlayer;
                log.Info($"{context.InvokingPlayer.Name} successfully changed password of their account {target.Session.Account.Email}.");
                HandleAccountChangePass(context, target.Session.Account.Email, password);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in AccountCommandCategory.HandleAccountChangeMyPass!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }
        [Command(Permission.Account, "Change the password of an account.", "changepass")]
        public void HandleAccountChangePass(ICommandContext context,
            [Parameter("Email address of the account to change")]
            string email,
            [Parameter("New password")]
            string password)
        {
            try
            {
                (string salt, string verifier) = PasswordProvider.GenerateSaltAndVerifier(email, password);
                if (email == null || email.Length <= 0)
                {
                    context.SendError("Account name wasn't specified properly.");
                    return;
                }
                DatabaseManager.Instance.AuthDatabase.ChangeAccountPassword(email, salt, verifier);

                if (context.InvokingPlayer != null)
                {
                    log.Info($"Account {email} password changed successfully by {context.InvokingPlayer.Name} ({context.InvokingPlayer.Session.Account.Email}).");
                }
                else
                {
                    log.Info($"Account {email} password changed successfully.");
                }
                context.SendMessage($"Account {email} successfully changed!");
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in AccountCommandCategory.HandleAccountChangePass!\n{e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.AccountDelete, "Delete an account.", "delete")]
        public void HandleAccountDelete(ICommandContext context,
            [Parameter("Email address of the account to delete")]
            string email)
        {
            try
            {
                if (DatabaseManager.Instance.AuthDatabase.DeleteAccount(email))
                {
                    if (context.InvokingPlayer != null)
                    {
                        log.Info($"Account {email} deleted successfully by {context.InvokingPlayer.Name} ({context.InvokingPlayer.Session.Account.Email}).");
                    }
                    else
                    {
                        log.Info($"Account {email} deleted successfully.");
                    }
                    context.SendMessage($"Account {email} successfully removed!");
                }
                else
                    context.SendMessage($"Cannot find account with Email: {email}");
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in AccountCommandCategory.HandleAccountDelete!\n{e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.AccountPassword, "Change a password for this account.", "password")]
        public void HandleAccountPassword(ICommandContext context,
            [Parameter("Password for the account.")]
            string password,
            [Parameter("Confirm password for the account.")]
            string confirm)
        {
            if (password != confirm)
            {
                context.SendMessage("Password and confirmation must match. Please try again.");
                return;
            }

            if (password.Length < 8)
            {
                context.SendMessage("Password must be at least 8 characters long. Please try another.");
                return;
            }

            string email = (context.Invoker as Player).Session.Account.Email;
            (string salt, string verifier) = PasswordProvider.GenerateSaltAndVerifier(email, password);
            DatabaseManager.Instance.AuthDatabase.SetPasswordForAccount(email, salt, verifier);

            context.SendMessage($"Account password changed.");
        }

        [Command(Permission.AccountAdminPassword, "Change a password for a given account.", "changepassword")]
        public void HandleAccountPassword(ICommandContext context,
            [Parameter("Username of the account.")]
            string email,
            [Parameter("Password for the account.")]
            string password,
            [Parameter("Confirm password for the account.")]
            string confirm)
        {
            if (!DatabaseManager.Instance.AuthDatabase.AccountExists(email))
            {
                context.SendMessage("Account not found. Please confirm username and try again.");
                return;
            }

            if (password != confirm)
            {
                context.SendMessage("Password and confirmation must match. Please try again.");
                return;
            }

            if (password.Length < 8)
            {
                context.SendMessage("Password must be at least 8 characters long. Please try another.");
                return;
            }

            (string salt, string verifier) = PasswordProvider.GenerateSaltAndVerifier(email, password);
            DatabaseManager.Instance.AuthDatabase.SetPasswordForAccount(email, salt, verifier);

            context.SendMessage($"Account password changed.");
        }

        [Command(Permission.AccountInventory, "A collection of commands to manage account inventory.", "inventory")]
        public class AccountInventoryCommandCategory : CommandCategory
        {
            [Command(Permission.AccountInventoryItemAdd, "Add an item to the account inventory.", "add")]
            public void HandleAccountInventoryCommandItemAdd(ICommandContext context,
                [Parameter("Item to add")]
                uint itemId,
                [Parameter("Item quantity")]
                uint? quantity)
            {
                quantity ??= 1u;

                if (context.GetTargetOrInvoker<Player>() == null)
                {
                    context.SendMessage("You need to have a target to add an Account Item to!");
                    return;
                }

                AccountItemEntry accountItem = GameTableManager.Instance.AccountItem.GetEntry(itemId);
                if (accountItem == null)
                {
                    context.SendMessage($"Could not find Account Item with ID {itemId}. Please try again with a valid ID.");
                    return;
                }

                context.GetTargetOrInvoker<Player>().Session.AccountInventory.ItemCreate(accountItem);
            }
        }
    }
}
