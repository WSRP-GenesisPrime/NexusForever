using NexusForever.Database.Character.Model;
using NexusForever.Database.World.Model;
using NexusForever.Shared.Database;
using NexusForever.WorldServer.Game.RealmTask.Static;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Helper
{
    public abstract class RealmTaskHelper
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public static uint HandleTask(RealmTaskModel realmTask)
        {
            try
            {
                if (realmTask.Type == (uint)RealmTaskType.CharacterRename)
                {
                    return HandleCharacterRename(realmTask);
                }
                else if (realmTask.Type == (uint)RealmTaskType.AccountCharacterTransfer)
                {
                    return HandleAccountCharacterTransfer(realmTask);
                }
                else if (realmTask.Type == (uint)RealmTaskType.GuildRename)
                {
                    return HandleGuildRename(realmTask);
                }
                //Inventory clear and class change will also be handled on login- whichever happens sooner
                else if (realmTask.Type == (uint)RealmTaskType.CharacterInventoryClear)
                {
                    return HandleCharacterInventoryClear(realmTask);
                }
                else if (realmTask.Type == (uint)RealmTaskType.CharacterClassChange)
                {
                    return HandleCharacterClassChange(realmTask);
                }
                //Neighbor request and guild invite reminders are exclusively handled on login
            }
            catch (Exception e)
            {
                log.Error(e.Message + ":\n" + e.StackTrace + ":\n" + e.InnerException);
            }

            return (uint) RealmTaskStatus.Retry;
        }

        private static uint HandleCharacterGuildInvite(RealmTaskModel realmTask)
        {
            throw new NotImplementedException();
        }

        private static uint HandleCharacterNeighborRequest(RealmTaskModel realmTask)
        {
            throw new NotImplementedException();
        }

        private static uint HandleCharacterClassChange(RealmTaskModel realmTask)
        {
            throw new NotImplementedException();
        }

        private static uint HandleCharacterInventoryClear(RealmTaskModel realmTask)
        {
            throw new NotImplementedException();
        }

        private static uint HandleGuildRename(RealmTaskModel realmTask)
        {
            throw new NotImplementedException();
        }

        private static uint HandleAccountCharacterTransfer(RealmTaskModel realmTask)
        {
            throw new NotImplementedException();
        }

        private static uint HandleCharacterRename(RealmTaskModel realmTask)
        {
            string newName = realmTask.Value;
            uint characterId = realmTask.CharacterId;

            try
            {
                DatabaseManager.Instance.CharacterDatabase.UpdateCharacterName(newName, characterId);
                realmTask.Status = (byte) RealmTaskStatus.Completed;
                log.Info($"Completed Realm Task: Character Rename (id: {characterId}, newName: {newName}");
            }
            catch (Exception e)
            {
                realmTask.Status = (byte) RealmTaskStatus.Failed;
                realmTask.StatusDescription = e.Message + "/" + e.InnerException;
            }
            SetAuditInfo(ref realmTask);
            DatabaseManager.Instance.WorldDatabase.UpdateRealmTask(realmTask);

            return realmTask.Status;
        }

        private static void SetAuditInfo(ref RealmTaskModel realmTask)
        {
            realmTask.LastRunTime = DateTime.Now;
        }
    }
}
