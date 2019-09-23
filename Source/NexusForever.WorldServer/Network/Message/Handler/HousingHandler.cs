using System;
using System.Numerics;
using System.Threading.Tasks;
using NexusForever.Shared.Game.Events;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Housing.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class HousingHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientHousingResidencePrivacyLevel)]
        public static void HandleHousingSetPrivacyLevel(WorldSession session, ClientHousingSetPrivacyLevel housingSetPrivacyLevel)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            residenceMap.SetPrivacyLevel(session.Player, housingSetPrivacyLevel);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingCrateAllDecor)]
        public static void HandleHousingCrateAllDecor(WorldSession session, ClientHousingCrateAllDecor housingCrateAllDecor)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            residenceMap.CrateAllDecor(session.Player);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingRemodel)]
        public static void HandleHousingRemodel(WorldSession session, ClientHousingRemodel housingRemodel)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            residenceMap.Remodel(session.Player, housingRemodel);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingDecorUpdate)]
        public static void HandleHousingDecorUpdate(WorldSession session, ClientHousingDecorUpdate housingDecorUpdate)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            residenceMap.DecorUpdate(session.Player, housingDecorUpdate);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingFlagsUpdate)]
        public static void HandleHousingFlagsUpdate(WorldSession session, ClientHousingFlagsUpdate flagsUpdate)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            residenceMap.UpdateResidenceFlags(session.Player, flagsUpdate);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingPlugUpdate)]
        public static void HandleHousingPlugUpdate(WorldSession session, ClientHousingPlugUpdate housingPlugUpdate)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            switch (housingPlugUpdate.Operation)
            {
                case PlugUpdateOperation.Place:
                    residenceMap.SetPlug(session.Player, housingPlugUpdate);
                    break;
                case PlugUpdateOperation.Remove:
                    residenceMap.RemovePlug(session.Player, housingPlugUpdate);
                    break;
                default:
                    log.Warn($"Operation {housingPlugUpdate.Operation} is unhandled.");
                    break;
            }
        }

        [MessageHandler(GameMessageOpcode.ClientHousingVendorList)]
        public static void HandleHousingVendorList(WorldSession session, ClientHousingVendorList housingVendorList)
        {
            var serverHousingVendorList = new ServerHousingVendorList
            {
                ListType = 0
            };
            
            // TODO: this isn't entirely correct
            foreach (HousingPlugItemEntry entry in GameTableManager.Instance.HousingPlugItem.Entries)
            {
                serverHousingVendorList.PlugItems.Add(new ServerHousingVendorList.PlugItem
                {
                    PlugItemId = entry.Id
                });
            }
            
            session.EnqueueMessageEncrypted(serverHousingVendorList);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingRenameProperty)]
        public static void HandleHousingRenameProperty(WorldSession session, ClientHousingRenameProperty housingRenameProperty)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            // TODO: validate name
            residenceMap.Rename(session.Player, housingRenameProperty);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingRandomCommunityList)]
        public static void HandleHousingRandomCommunityList(WorldSession session, ClientHousingRandomCommunityList housingRandomCommunityList)
        {
            session.EnqueueMessageEncrypted(new ServerHousingRandomCommunityList
            {
                Communities =
                {
                    new ServerHousingRandomCommunityList.Community
                    {
                        RealmId        = WorldServer.RealmId,
                        NeighborhoodId = 123,
                        Name           = "Blame Maxtor for working on WoW instead",
                        Owner          = "Not Yet Implemented!"
                    }
                }
            });
        }

        [MessageHandler(GameMessageOpcode.ClientHousingRandomResidenceList)]
        public static void HandleHousingRandomResidenceList(WorldSession session, ClientHousingRandomResidenceList housingRandomResidenceList)
        {
            var serverHousingRandomResidenceList = new ServerHousingRandomResidenceList();
            foreach (PublicResidence residence in ResidenceManager.Instance.GetRandomVisitableResidences())
            {
                serverHousingRandomResidenceList.Residences.Add(new ServerHousingRandomResidenceList.Residence
                {
                    RealmId     = WorldServer.RealmId,
                    ResidenceId = residence.ResidenceId,
                    Owner       = residence.Owner,
                    Name        = residence.Name
                });
            }

            session.EnqueueMessageEncrypted(serverHousingRandomResidenceList);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingVisit)]
        public static void HandleHousingVisit(WorldSession session, ClientHousingVisit housingVisit)
        {
            if (!(session.Player.Map is ResidenceMap))
                throw new InvalidPacketValueException();

            if (!session.Player.CanTeleport())
                return;

            Task<Residence> residenceTask;
            if (housingVisit.TargetResidenceName != "")
                residenceTask = ResidenceManager.Instance.GetResidence(housingVisit.TargetResidenceName);
            else if (housingVisit.TargetResidence.ResidenceId != 0ul)
                residenceTask = ResidenceManager.Instance.GetResidence(housingVisit.TargetResidence.ResidenceId);
            else
                throw new NotImplementedException();

            session.EnqueueEvent(new TaskGenericEvent<Residence>(residenceTask,
                residence =>
            {
                if (residence == null)
                {
                    // TODO: show error
                    return;
                }

                switch (residence.PrivacyLevel)
                {
                    case ResidencePrivacyLevel.Private:
                    {
                        // TODO: show error
                        return;
                    }
                    // TODO: check if player is either a neighbour or roommate
                    case ResidencePrivacyLevel.NeighborsOnly:
                        break;
                    case ResidencePrivacyLevel.RoommatesOnly:
                        break;
                }

                // teleport player to correct residence instance
                ResidenceEntrance entrance = ResidenceManager.Instance.GetResidenceEntrance(residence);
                session.Player.TeleportTo(entrance.Entry, entrance.Position, 0u, residenceId: residence.Id);
            }));
        }

        [MessageHandler(GameMessageOpcode.ClientHousingEditMode)]
        public static void HandleHousingEditMode(WorldSession session, ClientHousingEditMode housingEditMode)
        {
        }

        [MessageHandler(GameMessageOpcode.Client0721)]
        public static void Handle0721(WorldSession session, Client0721 client0721)
        {
            session.EnqueueMessageEncrypted(new Server022C
            {
                Unknown0 = true,
                Unknown1 = session.Player.Rotation.X,
                Unknown2 = -0f
            });
        }
        
        [MessageHandler(GameMessageOpcode.ClientHousingPropUpdate)]
        public static void HandleHousingDecorPropRequest(WorldSession session, ClientHousingPropUpdate propRequest)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            log.Info($"{propRequest.Operation}");

            switch (propRequest.Operation)
            {
                case 0:
                    residenceMap.RequestDecorEntity(session.Player, propRequest);
                    break;
                case 1:
                    residenceMap.CreateOrMoveDecorEntity(session.Player, propRequest);
                    break;
                case 2:
                    residenceMap.DeleteDecorEntity(session.Player, propRequest);
                    break;
            }
            
        }

        [MessageHandler(GameMessageOpcode.ClientHousingEnterInside)]
        public static void HandleHousingEnterinside(WorldSession session, ClientHousingEnterInside enterInside)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            if (!session.Player.CanUseHousingDoors())
            {
                session.EnqueueMessageEncrypted(new ServerHousingResult
                {
                    RealmId = WorldServer.RealmId,
                    ResidenceId = residenceMap.residence.Id,
                    PlayerName = session.Player.Name,
                    Result = HousingResult.Failed
                });
                return;
            }

            if (session.Player.HouseOutsideLocation != Vector3.Zero || session.Player.Position.Y < -720f)
            {
                Vector3 location = session.Player.HouseOutsideLocation;
                session.Player.HouseOutsideLocation = Vector3.Zero;
                if (location == Vector3.Zero)
                {
                    ResidenceEntrance entrance = ResidenceManager.Instance.GetResidenceEntrance(residenceMap.residence);
                    session.Player.TeleportTo(entrance.Entry, entrance.Position, 0u, residenceId: residenceMap.residence.Id);
                }
                else
                {
                    session.Player.MovementManager.SetRotation(new Vector3(-90f, 0f, 0f));
                    session.Player.MovementManager.SetPosition(location);
                }
                return;
            }

            Vector3 teleportPosition = ResidenceManager.Instance.GetResidenceInsideLocation(residenceMap.ResidenceInfoId);
            if (teleportPosition != Vector3.Zero)
            {
                session.Player.HouseOutsideLocation = session.Player.Position;
                session.Player.MovementManager.SetRotation(new Vector3(90f, 0f, 0f));
                session.Player.MovementManager.SetPosition(teleportPosition);
            }
            else
                session.Player.SendSystemMessage("Unknown teleport location.");
        }

        [MessageHandler(GameMessageOpcode.ClientHousingRemodelInterior)]
        public static void HandleHousingRemodelInterior(WorldSession session, ClientHousingRemodelInterior remodelInterior)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            residenceMap.DecorUpdate(session.Player, remodelInterior);
        }

        [MessageHandler(GameMessageOpcode.ClientHousingReturnHome)]
        public static void HandleHousingReturnHome(WorldSession session, ClientHousingReturnHome returnHome)
        {
            if (!(session.Player.Map is ResidenceMap residenceMap))
                throw new InvalidPacketValueException();

            Residence residence = ResidenceManager.Instance.GetResidence(session.Player.Name).GetAwaiter().GetResult();
            if (residence == null)
            {
                residence = ResidenceManager.Instance.CreateResidence(session.Player);
                
                if (residence == null)
                {
                    session.EnqueueMessageEncrypted(new ServerHousingResult
                    {
                        RealmId = WorldServer.RealmId,
                        ResidenceId = residenceMap.residence.Id,
                        PlayerName = session.Player.Name,
                        Result = HousingResult.Failed
                    });
                    return;
                }
            }

            ResidenceEntrance entrance = ResidenceManager.Instance.GetResidenceEntrance(residence);
            session.Player.TeleportTo(entrance.Entry, entrance.Position, 0u, residenceId: residence.Id);
        }
    }
}
