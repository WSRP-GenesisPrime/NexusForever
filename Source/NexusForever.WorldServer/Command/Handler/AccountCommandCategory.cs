using NexusForever.Shared.Configuration;
using NexusForever.Shared.Cryptography;
using NexusForever.Shared.Database;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;

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
            try
            {
                if (DatabaseManager.Instance.AuthDatabase.AccountExists(email))
                {
                    context.SendMessage("Account with that username already exists. Please try another.");
                    return;
                }
                
                role ??= (ConfigurationManager<WorldServerConfiguration>.Instance.Config.DefaultRole ?? (uint)Role.Player);
                
                (string salt, string verifier) = PasswordProvider.GenerateSaltAndVerifier(email, password, (uint)role);
                DatabaseManager.Instance.AuthDatabase.CreateAccount(email, salt, verifier);

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
            catch (Exception e)
            {
                log.Error($"Exception caught in AccountCommandCategory.HandleAccountCreate!\n{e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.AccountChangePass, "Change the password of an account.", "changepass")]
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

        [Command(Permission.AccountChangeMyPass, "Change the password of your account.", "changemypass")]
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
    }
}
