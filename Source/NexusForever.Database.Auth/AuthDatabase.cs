using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Auth.Model;
using NexusForever.Database.Configuration;
using NLog;

namespace NexusForever.Database.Auth
{
    public class AuthDatabase
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly IDatabaseConfig config;

        public AuthDatabase(IDatabaseConfig config)
        {
            this.config = config;
        }

        public async Task Save(Action<AuthContext> action)
        {
            using var context = new AuthContext(config);
            action.Invoke(context);
            await context.SaveChangesAsync();
        }

        public void Migrate()
        {
            using var context = new AuthContext(config);

            List<string> migrations = context.Database.GetPendingMigrations().ToList();
            if (migrations.Count > 0)
            {
                log.Info($"Applying {migrations.Count} authentication database migration(s)...");
                foreach (string migration in migrations)
                    log.Info(migration);

                context.Database.Migrate();
            }
        }

        /// <summary>
        /// Selects an <see cref="AccountModel"/> asynchronously that matches the supplied email.
        /// </summary>
        public async Task<AccountModel> GetAccountByEmailAsync(string email)
        {
            using var context = new AuthContext(config);
            return await context.Account.SingleOrDefaultAsync(a => a.Email == email);
        }

        /// <summary>
        /// Selects an <see cref="AccountModel"/> asynchronously that matches the supplied email and game token.
        /// </summary>
        public async Task<AccountModel> GetAccountByGameTokenAsync(string email, string gameToken)
        {
            using var context = new AuthContext(config);
            return await context.Account.SingleOrDefaultAsync(a => a.Email == email && a.GameToken == gameToken);
        }

        /// <summary>
        /// Selects an <see cref="AccountModel"/> asynchronously that matches the supplied email and session key.
        /// </summary>
        public async Task<AccountModel> GetAccountBySessionKeyAsync(string email, string sessionKey)
        {
            using var context = new AuthContext(config);
            return await context.Account
                .AsSplitQuery()
                .Include(a => a.AccountCostumeUnlock)
                .Include(a => a.AccountCurrency)
                .Include(a => a.AccountEntitlement)
                .Include(a => a.AccountGenericUnlock)
                .Include(a => a.AccountItem)
                .Include(a => a.AccountItemCooldown)
                .Include(a => a.AccountKeybinding)
                .Include(a => a.AccountEntitlement)
                .Include(a => a.AccountPermission)
                .Include(a => a.AccountRole)
                .Include(a => a.AccountRewardTrack)
                    .ThenInclude(b => b.Milestone)
                .Include(a => a.AccountStoreTransaction)
                .Include(a => a.AccountLinkEntry)
                .SingleOrDefaultAsync(a => a.Email == email && a.SessionKey == sessionKey);
        }

        /// <summary>
        /// Returns if an account with the given username already exists.
        /// </summary>
        public bool AccountExists(string email)
        {
            using var context = new AuthContext(config);
            return context.Account.SingleOrDefault(a => a.Email == email) != null;
        }

        public bool AccountIsLinked(string email)
        {
            using var context = new AuthContext(config);

            AccountModel account = context.Account.SingleOrDefault(a => a.Email == email);
            return context.AccountLinkEntry.Any(a => a.AccountId == account.Id);
        }

        public bool AccountLinkExists(string link)
        {
            using var context = new AuthContext(config);
            return context.AccountLink.SingleOrDefault(a => a.Id == link) != null;
        }

        public List<uint> GetAccountCostumeUnlockItemIdsForSync(AccountModel account)
        {
            using var context = new AuthContext(config);

            List<uint> costumeItemIds = null;

            string accountLink = account.AccountLinkEntry.ToList().First().Id;
            List<AccountLinkEntryModel> accountLinkEntries = context.AccountLinkEntry
                .Include(a => a.Link)
                .Include(a => a.Account)
                //filter out the account that's initiating the sync
                .Where(a => a.Link.Id == accountLink && a.Account.Email != account.Email).AsSplitQuery()
                .ToList();

            if (accountLinkEntries != null)
            {
                costumeItemIds = new List<uint>();
                foreach (var accountLinkEntry in accountLinkEntries)
                {
                    List<AccountCostumeUnlockModel> costumeUnlocks = GetAccountCostumeUnlocks(accountLinkEntry.AccountId);
                    if (costumeUnlocks != null)
                    {
                        foreach (var costumeUnlock in costumeUnlocks)
                        {
                            if (costumeItemIds.Contains(costumeUnlock.Id))
                                continue;
                            costumeItemIds.Add(costumeUnlock.ItemId);
                        }
                    }
                }
            }

            return costumeItemIds;
        }
        public List<AccountCostumeUnlockModel> GetAccountCostumeUnlocks(uint accountId)
        {
            using var context = new AuthContext(config);
            return context.AccountCostumeUnlock
                .Include(a => a.Account)
                .Where(a => a.Account.Id == accountId).AsSplitQuery()
                .ToList();
        }

        /// <summary>
        /// Create a new account with the supplied email, salt and password verifier that is inserted into the database.
        /// </summary>
        public void CreateAccount(string email, string s, string v, uint role)
        {
            CreateAccount(email, s, v, role, null);
        }
        public void CreateAccount(string email, string s, string v, uint role, string link)
        {
            email = email.ToLower();
            if (AccountExists(email))
                throw new InvalidOperationException($"Account with that username already exists.");

            using var context = new AuthContext(config);
            var model = new AccountModel
            {
                Email = email,
                S = s,
                V = v,
            };
            model.AccountRole.Add(new AccountRoleModel
            {
                RoleId = role
            });

            if (link != null && link.Length > 0)
            {
                if (!AccountLinkExists(link))
                    throw new InvalidOperationException($"That account link does not exist.");
                
                model.AccountLinkEntry.Add(new AccountLinkEntryModel
                {
                    Id = link
                });
            }
            context.Account.Add(model);

            context.SaveChanges();
        }

        public void LinkAccount(string name, string link)
        {
            using var context = new AuthContext(config);
            var model = context.Account.SingleOrDefault(a => a.Email == name);
            if (model == null)
                throw new InvalidOperationException($"That account does not exist.");


            if (link != null && link.Length > 0)
            {
                if (!AccountLinkExists(link))
                    throw new InvalidOperationException($"That account link does not exist.");

                model.AccountLinkEntry.Add(new AccountLinkEntryModel
                {
                    Id = link
                });
            }
            context.Account.Update(model);

            context.SaveChanges();
        }

        public string LinkAccounts(string firstAccount, string secondAccount)
        {
            using var context = new AuthContext(config);

            AccountModel modelOne = context.Account.SingleOrDefault(a => a.Email == firstAccount);
            if (modelOne == null)
                throw new InvalidOperationException($"The first account does not exist.");

            AccountModel modelTwo = context.Account.SingleOrDefault(a => a.Email == secondAccount);
            if (modelTwo == null)
                throw new InvalidOperationException($"The second account does not exist.");

            AccountLinkModel accountLink = null;
            //Try to get the existing account link
            if (modelOne.AccountLinkEntry.ToList().Count > 0)
                accountLink = modelOne.AccountLinkEntry.ToList().First().Link;
            else if (modelTwo.AccountLinkEntry.ToList().Count > 0)
                accountLink = modelTwo.AccountLinkEntry.ToList().First().Link;

            log.Info($"accountLink= {accountLink}");
            //If no existing account link, create a new one
            if (accountLink == null)
            {
                char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
                byte[] data = new byte[4 * 8];
                using (var crypto = RandomNumberGenerator.Create())
                {
                    crypto.GetBytes(data);
                }
                StringBuilder result = new StringBuilder(8);
                for (int i = 0; i < 8; i++)
                {
                    var rnd = BitConverter.ToUInt32(data, i * 4);
                    var idx = rnd % chars.Length;

                    result.Append(chars[idx]);
                }
                string link = result.ToString();
                accountLink = CreateAccountLink(link, DateTime.Now, "Auto-generated by LinkTo", "AccountCommand");
            }

            modelOne.AccountLinkEntry.Add(new AccountLinkEntryModel
            {
                Id = accountLink.Id
            });
            context.Account.Update(modelOne);

            modelTwo.AccountLinkEntry.Add(new AccountLinkEntryModel
            {
                Id = accountLink.Id
            });
            context.Account.Update(modelTwo);

            context.SaveChanges();

            return accountLink.Id;
        }
        public AccountLinkModel CreateAccountLink(string link, DateTime createTime, string comment)
        {
            return CreateAccountLink(link, createTime, comment, "");
        }
        public AccountLinkModel CreateAccountLink(string link, DateTime createTime, string comment, string createdBy)
        {
            if (AccountLinkExists(link))
                throw new InvalidOperationException($"That account link already exists. Please try again.");

            using var context = new AuthContext(config);
            var model = new AccountLinkModel
            {
                Id = link,
                CreatedBy = createdBy,
                CreateTime = createTime,
                Comment = comment
            };
            context.AccountLink.Add(model);

            context.SaveChanges();

            return model;
        }

        /// <summary>
        /// Change the password of an account.
        /// </summary>
        public void ChangeAccountPassword(string email, string s, string v)
        {
            email = email.ToLower();
            if (!AccountExists(email))
                throw new InvalidOperationException($"Account with that username already exists.");

            using var context = new AuthContext(config);
            AccountModel account = context.Account.SingleOrDefault(a => a.Email == email);
            account.S = s;
            account.V = v;

            context.SaveChanges();
        }

        /// <summary>
        /// Delete an existing account with the supplied email.
        /// </summary>
        public bool DeleteAccount(string email)
        {
            using var context = new AuthContext(config);
            AccountModel account = context.Account.SingleOrDefault(a => a.Email == email);
            if (account == null)
                return false;

            context.Account.Remove(account);
            return context.SaveChanges() > 0;
        }

        /// <summary>
        /// Update <see cref="AccountModel"/> with supplied game token asynchronously.
        /// </summary>
        public async Task UpdateAccountGameToken(AccountModel account, string gameToken)
        {
            account.GameToken = gameToken;

            using var context = new AuthContext(config);
            EntityEntry<AccountModel> entity = context.Attach(account);
            entity.Property(p => p.GameToken).IsModified = true;
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Set the password combination of salt and verifier for a given account.
        /// </summary>
        public void SetPasswordForAccount(string email, string s, string v)
        {
            email = email.ToLower();
            if (!AccountExists(email))
                throw new InvalidOperationException($"Account with that username already exists.");

            using var context = new AuthContext(config);
            AccountModel account = context.Account.FirstOrDefault(a => a.Email == email);
            account.S = s;
            account.V = v;

            EntityEntry<AccountModel> entity = context.Attach(account);
            entity.Property(p => p.S).IsModified = true;
            entity.Property(p => p.V).IsModified = true;
            
            context.SaveChanges();
        }

        /// <summary>
        /// Update <see cref="AccountModel"/> with supplied session key asynchronously.
        /// </summary>
        public async Task UpdateAccountSessionKey(AccountModel account, string sessionKey)
        {
            account.SessionKey = sessionKey;

            await using var context = new AuthContext(config);
            EntityEntry<AccountModel> entity = context.Attach(account);
            entity.Property(p => p.SessionKey).IsModified = true;
            await context.SaveChangesAsync();
        }

        public ImmutableList<ServerModel> GetServers()
        {
            using var context = new AuthContext(config);
            return context.Server
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<ServerMessageModel> GetServerMessages()
        {
            using var context = new AuthContext(config);
            return context.ServerMessage
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<PermissionModel> GetPermissions()
        {
            using var context = new AuthContext(config);
            return context.Permission
                .AsNoTracking()
                .ToImmutableList();
        }

        public ImmutableList<RoleModel> GetRoles()
        {
            using var context = new AuthContext(config);
            return context.Role
                .Include(r => r.RolePermission)
                .AsNoTracking()
                .ToImmutableList();
        }
        
        public ulong GetNextAccountItemId()
        {
            using var context = new AuthContext(config);

            return context.AccountItem
                .Select(r => r.Id)
                .DefaultIfEmpty()
                .Max();
        }

        private async Task<ulong> GetNextTransactionId()
        {
            await using var context = new AuthContext(config);

            ulong id = context.AccountStoreTransaction
                .Select(r => r.TransactionId)
                .DefaultIfEmpty()
                .Max();

            return id < 10000000ul ? 10000000ul + 1ul : id + 1ul;
        }

        public async Task<AccountStoreTransactionModel> CreateStoreTransaction(AccountModel account, AccountStoreTransactionModel transaction)
        {
            ulong id = await GetNextTransactionId();

            transaction.TransactionId = id;

            await using var context = new AuthContext(config);
            EntityEntry<AccountModel> entity = context.Attach(account);
            context.Add(transaction);
            await context.SaveChangesAsync();

            return transaction;
        }
    }
}
