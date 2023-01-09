using NexusForever.Database.World.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Linq;
using System.Numerics;

namespace NexusForever.WorldServer.Game.Entity
{
    public class Ghost : UnitEntity
    {
        public Player Owner { get; }

        public Ghost(Player owner)
            : base(EntityType.Ghost)
        {
            Owner = owner;

            SetAppearance(owner.GetAppearance());
            Position = owner.Position;
            Rotation = owner.Rotation;
            Faction1 = owner.Faction1;
            Faction2 = owner.Faction2;

            CreateFlags |= EntityCreateFlag.SpawnAnimation;

            SetBaseProperty(Property.BaseHealth, 101.0f);

            SetStat(Stat.Health, 101u);
            SetStat(Stat.Level, owner.Level);
            SetStat(Stat.Sheathed, 1);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new GhostEntityModel
            {
                Name = Owner.Name,
                Race = Owner.Race,
                Class = Owner.Class,
                Sex = Owner.Sex,
                GuildIds = Owner.GuildManager
                    .Select(g => g.Id)
                    .ToList(),
                GuildName = Owner.GuildManager.GuildAffiliation?.Name,
                GuildType = Owner.GuildManager.GuildAffiliation?.Type ?? GuildType.None,
                Title = Owner.TitleManager.ActiveTitleId
            };
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            CreateFlags &= ~EntityCreateFlag.SpawnAnimation;
            CreateFlags |= EntityCreateFlag.NoSpawnAnimation;

            Owner.GhostGuid = guid;
            Owner.SetControl(this);
            Owner.Session.EnqueueMessageEncrypted(new ServerResurrectionShow
            {
                GhostId = Guid,
                RezCost = GetCostForRez(),
                TimeUntilRezMs = 5000,
                ShowRezFlags = MapManager.Instance.GetRezTypeForMap(Owner),
                Dead = true,
                Unknown0 = false,
                TimeUntilForceRezMs = 0,
                TimeUntilWakeHereMs = 0
            });
        }

        public uint GetCostForRez()
        {
            // TODO: Calculate credit cost correctly. 0 for now.
            return 0u;
        }
    }
}
