using System;
using System.Linq;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Command;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Loot;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game;
using NLog;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class EntityHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientEntityCommand)]
        public static void HandleEntityCommand(WorldSession session, ClientEntityCommand entityCommand)
        {
            WorldEntity mover = session.Player;
            if (mover == null)
                return;

            if (session.Player.ControlGuid != session.Player.Guid)
                mover = session.Player.GetVisible<WorldEntity>(session.Player.ControlGuid);

            if (mover == null)
                return;

            if (session.Player.IsEmoting)
                session.Player.IsEmoting = false;

            mover?.MovementManager.HandleClientEntityCommands(entityCommand.Commands, entityCommand.Time);
        }

        [MessageHandler(GameMessageOpcode.ClientActivateUnit)]
        public static void HandleActivateUnit(WorldSession session, ClientActivateUnit unit)
        {
            try
            {
                WorldEntity entity = session.Player.GetVisible<WorldEntity>(unit.UnitId);
                if (entity == null)
                    throw new InvalidPacketValueException();

                // TODO: sanity check for range etc.

                entity.OnInteract(session.Player);
            }
            catch (Exception e)
            {
                session.Player.SendSystemMessage("Error while interacting with unit!");
            }
        }

        [MessageHandler(GameMessageOpcode.ClientEntityInteract)]
        public static void HandleClientEntityInteraction(WorldSession session, ClientEntityInteract entityInteraction)
        {
            WorldEntity entity = session.Player.GetVisible<WorldEntity>(entityInteraction.Guid);
            if (entity != null)
            {
                session.Player.QuestManager.ObjectiveUpdate(QuestObjectiveType.TalkTo, entity.CreatureId, 1u);
                foreach (uint targetGroupId in AssetManager.Instance.GetTargetGroupsForCreatureId(entity.CreatureId) ?? Enumerable.Empty<uint>())
                    session.Player.QuestManager.ObjectiveUpdate(QuestObjectiveType.TalkToTargetGroup, targetGroupId, 1u);
            }

            switch (entityInteraction.Event)
            {
                case 37: // Quest NPC
                {
                    session.EnqueueMessageEncrypted(new Server0357
                    {
                        UnitId = entityInteraction.Guid
                    });
                    break;
                }
                case 49: // Handle Vendor
                    VendorHandler.HandleClientVendor(session, entityInteraction);
                    break;
                case 68: // "MailboxActivate"
                    var mailboxEntity = session.Player.Map.GetEntity<Mailbox>(entityInteraction.Guid);
                    break;
                case 8: // "HousingGuildNeighborhoodBrokerOpen"
                case 40:
                case 41: // "ResourceConversionOpen"
                case 42: // "ToggleAbilitiesWindow"
                case 43: // "InvokeTradeskillTrainerWindow"
                case 45: // "InvokeShuttlePrompt"
                case 46:
                case 47:
                case 48: // "InvokeTaxiWindow"
                case 65: // "MannequinWindowOpen"
                case 66: // "ShowBank"
                case 67: // "ShowRealmBank"
                case 69: // "ShowDye"
                case 70: // "GuildRegistrarOpen"
                case 71: // "WarPartyRegistrarOpen"
                case 72: // "GuildBankerOpen"
                case 73: // "WarPartyBankerOpen"
                case 75: // "ToggleMarketplaceWindow"
                case 76: // "ToggleAuctionWindow"
                case 79: // "TradeskillEngravingStationOpen"
                case 80: // "HousingMannequinOpen"
                case 81: // "CityDirectionsList"
                case 82: // "ToggleCREDDExchangeWindow"
                case 84: // "CommunityRegistrarOpen"
                case 85: // "ContractBoardOpen"
                case 86: // "BarberOpen"
                case 87: // "MasterCraftsmanOpen"
                default:
                    log.Warn($"Received unhandled interaction event {entityInteraction.Event} from Entity {entityInteraction.Guid}");
                    break;
            }
        }

        [MessageHandler(GameMessageOpcode.ClientEntityInteractChair)]
        public static void HandleClientEntityInteractEmote(WorldSession session, ClientEntityInteractChair interactChair)
        {
            WorldEntity chair = session.Player.GetVisible<WorldEntity>(interactChair.ChairUnitId);
            if (chair == null)
                throw new InvalidPacketValueException();

            Creature2Entry creatureEntry = GameTableManager.Instance.Creature2.GetEntry(chair.CreatureId);
            if ((creatureEntry.ActivationFlags & 0x200000) == 0)
                throw new InvalidPacketValueException();

            session.Player.Sit(chair);
        }

        [MessageHandler(GameMessageOpcode.ClientActivateUnitCast)]
        public static void HandleActivateUnitCast(WorldSession session, ClientActivateUnitCast request)
        {
            WorldEntity entity = session.Player.GetVisible<WorldEntity>(request.ActivateUnitId);
            if (entity == null)
                throw new InvalidPacketValueException();

            entity.OnActivateCast(session.Player, request.ClientUniqueId);
        }

        /// <remarks>
        /// Possibly only used by Bindpoint entities
        /// </remarks>
        [MessageHandler(GameMessageOpcode.ClientActivateUnitInteraction)]
        public static void HandleActivateUnitDeferred(WorldSession session, ClientActivateUnitInteraction request)
        {

            WorldEntity entity = session.Player.GetVisible<WorldEntity>(request.ActivateUnitId);
            if (entity == null)
                throw new InvalidPacketValueException();

            entity.OnActivateCast(session.Player, request.ClientUniqueId);
        }

        [MessageHandler(GameMessageOpcode.ClientInteractionResult)]
        public static void HandleSpellDeferredResult(WorldSession session, ClientSpellInteractionResult result)
        {
            if (!(session.Player.HasSpell(x => x.CastingId == result.CastingId, out Spell spell)))
                throw new ArgumentNullException($"Spell cast {result.CastingId} not found.");

            if (spell is not SpellClientSideInteraction spellCSI)
                throw new ArgumentNullException($"Spell missing a ClientSideInteraction.");

            switch (result.Result)
            {
                case 0:
                    spellCSI.FailClientInteraction();
                    break;
                case 1:
                    spellCSI.SucceedClientInteraction();
                    break;
                case 2:
                    spellCSI.CancelCast(Game.Spell.Static.CastResult.ClientSideInteractionFail);
                    break;
            }
        }

        [MessageHandler(GameMessageOpcode.ClientMovementSpeedUpdate)]
        public static void HandleMovementSpeedChange(WorldSession session, ClientMovementSpeedUpdate speedUpdate)
        {
            session.Player.HandleMovementSpeedApply(speedUpdate.Speed);
        }

        [MessageHandler(GameMessageOpcode.ClientMovementSpeedSprint)]
        public static void HandleMovementSpeedSprint(WorldSession session, ClientMovementSpeedSprint speedSprint)
        {
            session.Player.HandleMovementSprintRequest(speedSprint.Sprint);
        }

        [MessageHandler(GameMessageOpcode.ClientDash)]
        public static void HandleClientDash(WorldSession session, ClientDash clientDash)
        {
            session.Player.HandleDash(clientDash.Direction);
        }

        [MessageHandler(GameMessageOpcode.ClientResurrectRequest)]
        public static void HandleClientResurrectRequest(WorldSession session, ClientResurrectRequest clientResurrectRequest)
        {
            WorldEntity entity = session.Player.Map.GetEntity<WorldEntity>(clientResurrectRequest.UnitId);
            if (entity != null)
            {
                if (entity is Player player && session.Player.Guid == entity.Guid)
                {
                    player.DoResurrect(clientResurrectRequest.RezType);
                    return;
                }
                else
                    throw new InvalidOperationException($"Player requested resurrection of an entity that they do not own!");
            }

            throw new NotImplementedException($"Targeted Entity not found. Can players choose what type of resurrection other players get?");
        }

        [MessageHandler(GameMessageOpcode.ClientLootVacuum)]
        public static void HandleClientLootVacuum(WorldSession session, ClientLootVacuum clientLootVacuum)
        {
            GlobalLootManager.Instance.GiveAllLootInRange(session);
        }

        [MessageHandler(GameMessageOpcode.ClientLootItem)]
        public static void HandleClientLootItem(WorldSession session, ClientLootItem clientLootItem)
        {
            GlobalLootManager.Instance.GiveLoot(session, (int)clientLootItem.LootId);
        }
    }
}
