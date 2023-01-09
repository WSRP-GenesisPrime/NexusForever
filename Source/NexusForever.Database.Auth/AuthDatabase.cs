﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        /// <summary>
        /// Create a new account with the supplied email, salt and password verifier that is inserted into the database.
        /// </summary>
        public void CreateAccount(string email, string s, string v, uint role)
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
            context.Account.Add(model);

            context.SaveChanges();
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
