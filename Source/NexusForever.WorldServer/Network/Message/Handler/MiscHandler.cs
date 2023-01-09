using System;
using System.Collections.Generic;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Contact.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map.Search;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class MiscHandler
    {
        private const float LocalChatDistance = 175f;
        [MessageHandler(GameMessageOpcode.ClientPing)]
        public static void HandlePing(WorldSession session, ClientPing ping)
        {
            session.Heartbeat.OnHeartbeat();
        }

        /// <summary>
        /// Handled responses to Player Info Requests.
        /// TODO: Put this in the right place, this is used by Mail & Contacts, at minimum. Probably used by Guilds, Circles, etc. too.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="request"></param>
        [MessageHandler(GameMessageOpcode.ClientPlayerInfoRequest)]
        public static void HandlePlayerInfoRequest(WorldSession session, ClientPlayerInfoRequest request)
        {
            ICharacter character = CharacterManager.Instance.GetCharacterInfo(request.Identity.CharacterId);
            if (character == null)
                throw new InvalidPacketValueException();
            
            float? onlineStatus = character.GetOnlineStatus();
            if (request.Type == ContactType.Ignore) // Ignored user data request
                session.EnqueueMessageEncrypted(new ServerPlayerInfoBasicResponse
                {
                    ResultCode = 0,
                    Identity = new TargetPlayerIdentity
                    {
                        RealmId = WorldServer.RealmId,
                        CharacterId = character.CharacterId
                    },
                    Name = character.Name,
                    Faction = character.Faction1,
                });
            else
                session.EnqueueMessageEncrypted(new ServerPlayerInfoFullResponse
                {
                    BaseData = new ServerPlayerInfoBasicResponse
                    {
                        ResultCode = 0,
                        Identity = new TargetPlayerIdentity
                        {
                            RealmId = WorldServer.RealmId,
                            CharacterId = character.CharacterId
                        },
                        Name = character.Name,
                        Faction = character.Faction1
                    },
                    IsClassPathSet = true,
                    Path = character.Path,
                    Class = character.Class,
                    Level = character.Level,
                    IsLastLoggedOnInDaysSet = onlineStatus.HasValue,
                    LastLoggedInDays = onlineStatus.GetValueOrDefault(0f)
                });
        }

        [MessageHandler(GameMessageOpcode.ClientToggleWeapons)]
        public static void HandleWeaponToggle(WorldSession session, ClientToggleWeapons toggleWeapons)
        {
            session.Player.Sheathed = toggleWeapons.ToggleState;
        }

        [MessageHandler(GameMessageOpcode.ClientRandomRollRequest)]
        public static void HandleRandomRoll(WorldSession session, ClientRandomRollRequest randomRoll)
        {
            if (randomRoll.MinRandom > randomRoll.MaxRandom)
                throw new InvalidPacketValueException();

            if (randomRoll.MaxRandom > 1000000u)
                throw new InvalidPacketValueException();

            int RandomRollResult = new Random().Next((int)randomRoll.MinRandom, (int)randomRoll.MaxRandom+1);

            // get players in local chat range
            session.Player.Map.Search(
                session.Player.Position,
                GlobalChatManager.LocalChatDistance,
                new SearchCheckRangePlayerOnly(session.Player.Position, GlobalChatManager.LocalChatDistance, session.Player),
                out List<GridEntity> intersectedEntities
            );

            string systemMessage = $"{session.Player.Name} rolls {RandomRollResult} ({randomRoll.MinRandom}-{randomRoll.MaxRandom})";
            intersectedEntities.ForEach(e => ((Player)e).SendSystemMessage(systemMessage));

            session.EnqueueMessageEncrypted(new ServerRandomRollResponse
            {
                TargetPlayerIdentity = new TargetPlayerIdentity
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = session.Player.CharacterId
                },
                MinRandom = randomRoll.MinRandom,
                MaxRandom = randomRoll.MaxRandom,
                RandomRollResult = RandomRollResult
            });
        }

        [MessageHandler(GameMessageOpcode.ClientZoneChange)]
        public static void HandleClientZoneChange(WorldSession session, ClientZoneChange zoneChange)
        {
        }

        /// <summary>
        /// Client sends this when it has received everything it needs to leave the loading screen.
        /// For housing maps, this also includes things such as residences and plots.
        /// See 0x732990 in the client for more information.
        /// </summary>
        [MessageHandler(GameMessageOpcode.ClientEnteredWorld)]
        public static void HandleClientEnteredWorld(WorldSession session, ClientEnteredWorld enteredWorld)
        {
            if (!session.Player.IsLoading)
                throw new InvalidPacketValueException();

            session.Player.OnEnterWorld();
        }
        
        [MessageHandler(GameMessageOpcode.ClientCinematicState)]
        public static void HandleCinematicState(WorldSession session, ClientCinematicState cinematicState)
        {
            session.Player.CinematicManager.HandleClientCinematicState(cinematicState.State);
        }

        [MessageHandler(GameMessageOpcode.ClientRewardTrackChoice)]
        public static void HandleClickRewardTrachChoice(WorldSession session, ClientRewardTrackChoice rewardTrackChoice)
        {
            RewardTrackRewardsEntry rewardEntry = GameTableManager.Instance.RewardTrackRewards.GetEntry(rewardTrackChoice.RewardId);
            if (rewardEntry == null)
                throw new InvalidOperationException($"RewardTrackRewards entry with ID {rewardTrackChoice.RewardId} not found!");

            session.RewardTrackManager.HandleChooseReward(rewardEntry, rewardTrackChoice.Index);
        }
    }
}
