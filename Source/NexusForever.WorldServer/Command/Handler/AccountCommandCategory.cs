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
            uint? role = null)
        {
            if (DatabaseManager.Instance.AuthDatabase.AccountExists(email))
            {
                context.SendMessage("Account with that username already exists. Please try another.");
                return;
            }
                
            role ??= (ConfigurationManager<WorldServerConfiguration>.Instance.Config.DefaultRole ?? (uint)Role.Player);
                
            (string salt, string verifier) = PasswordProvider.GenerateSaltAndVerifier(email, password);
            DatabaseManager.Instance.AuthDatabase.CreateAccount(email, salt, verifier, (uint)role);

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

        [Command(Permission.AccountDelete, "Delete an account.", "delete")]
        public void HandleAccountDelete(ICommandContext context,
            [Parameter("Email address of the account to delete")]
            string email)
        {
            if (DatabaseManager.Instance.AuthDatabase.DeleteAccount(email))
                context.SendMessage($"Account {email} successfully removed!");
            else
                context.SendMessage($"Cannot find account with Email: {email}");
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
