using System.Linq;
using System.Numerics;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Game.Entity
{
    public class VanityPet : WorldEntity
    {
        private float FollowDistance { get; set; } = 3f;
        private float FollowMinRecalculateDistance = 5f;

        private bool FollowingPlayer { get; set; } = true;
        private bool FacingPlayer { get; set; } = true;
        private bool FollowingOnSide { get; set; } = false;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public uint OwnerGuid { get; private set; }
        public Creature2Entry Creature { get; }
        public Creature2DisplayGroupEntryEntry Creature2DisplayGroup { get; }

        private readonly UpdateTimer followTimer = new(1d);

        public VanityPet(Player owner, uint creature)
            : base(EntityType.Pet)
        {
            OwnerGuid               = owner.Guid;
            Creature                = GameTableManager.Instance.Creature2.GetEntry(creature);
            Creature2DisplayGroup   = GameTableManager.Instance.Creature2DisplayGroupEntry.Entries.SingleOrDefault(x => x.Creature2DisplayGroupId == Creature.Creature2DisplayGroupId);
            DisplayInfo             = Creature2DisplayGroup?.Creature2DisplayInfoId ?? 0u;

            SetBaseProperty(Property.BaseHealth, 800.0f);

            SetStat(Stat.Health, 800u);
            SetStat(Stat.Level, 3u);
            SetStat(Stat.Sheathed, 0u);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new PetEntityModel
            {
                CreatureId  = Creature.Id,
                OwnerId     = OwnerGuid,
                Name        = ""
            };
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            Player owner = GetVisible<Player>(OwnerGuid);
            if (owner == null)
            {
                // this shouldn't happen, log it anyway
                log.Error($"VanityPet {Guid} has lost it's owner {OwnerGuid}!");
                RemoveFromMap();
                return;
            }

            owner.VanityPetGuid = Guid;

            owner.EnqueueToVisible(new Server08B3
            {
                MountGuid = Guid,
                Unknown0  = 0,
                Unknown1  = true
            }, true);
        }

        public override void OnEnqueueRemoveFromMap()
        {
            followTimer.Reset(false);
            OwnerGuid = 0u;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (IsFollowingPlayer())
            {
                Follow(lastTick, FollowingOnSide, FacingPlayer);
            }
        }

        private void Follow(double lastTick)
        {
            followTimer.Update(lastTick);
            if (!followTimer.HasElapsed)
                return;

            Player owner = GetVisible<Player>(OwnerGuid);
            if (owner == null)
            {
                // this shouldn't happen, log it anyway
                log.Error($"VanityPet {Guid} has lost it's owner {OwnerGuid}!");
                RemoveFromMap();
                return;
            }

            // only recalculate the path to owner if distance is significant
            float distance = owner.Position.GetDistance(Position);
            if (distance < FollowMinRecalculateDistance)
                return;

            MovementManager.Follow(owner, FollowDistance);

            followTimer.Reset();
        }

        private void Follow(double lastTick, bool sideAngle, bool facePlayer)
        {
            followTimer.Update(lastTick);
            if (!followTimer.HasElapsed)
                return;

            Player owner = GetVisible<Player>(OwnerGuid);
            if (owner == null)
            {
                // this shouldn't happen, log it anyway
                log.Error($"VanityPet {Guid} has lost it's owner {OwnerGuid}!");
                RemoveFromMap();
                return;
            }

            // only recalculate the path to owner if distance is significant
            float distance = owner.Position.GetDistance(Position);
            if (distance < FollowMinRecalculateDistance)
                return;

            MovementManager.Follow(owner, FollowDistance, sideAngle, facePlayer);

            followTimer.Reset();
        }

        public bool IsFollowingPlayer()
        {
            return this.FollowingPlayer;
        }
        public void SetIsFollowingPlayer(bool isFollowing)
        {
            this.FollowingPlayer = isFollowing;
        }

        public bool IsFacingPlayer()
        {
            return this.FacingPlayer;
        }
        public void SetIsFacingPlayer(bool isFacing)
        {
            this.FacingPlayer = isFacing;
        }

        public float GetFollowDistance()
        {
            return this.FollowDistance;
        }
        public void SetFollowDistance(float dist)
        {
            this.FollowDistance = dist;
        }
        public void SetFollowFollowMinRecalculateDistance(float mindist)
        {
            this.FollowMinRecalculateDistance = mindist;
        }

        public bool IsFollowingOnSide()
        {
            return this.FollowingOnSide;
        }
        public void SetFollowingOnSide(bool isFollowingOnSide)
        {
            this.FollowingOnSide = isFollowingOnSide;
        }
    }
}
