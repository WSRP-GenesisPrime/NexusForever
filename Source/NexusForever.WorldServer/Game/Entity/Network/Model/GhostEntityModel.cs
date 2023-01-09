using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Guild.Static;

namespace NexusForever.WorldServer.Game.Entity.Network.Model
{
    public class GhostEntityModel : IEntityModel
    {
        public ulong Id { get; set; }
        public ushort RealmId { get; set; }
        public string Name { get; set; }
        public Race Race { get; set; }
        public Class Class { get; set; }
        public Sex Sex { get; set; }
        public ulong GroupId { get; set; }
        public string GuildName { get; set; }
        public GuildType GuildType { get; set; }
        public List<ulong> GuildIds { get; set; } = new List<ulong>();
        public List<float> Bones { get; set; } = new List<float>();
        public ushort Title { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Id);
            writer.Write(RealmId, 14u);
            writer.WriteStringWide(Name);
            writer.Write(Race, 5u);
            writer.Write(Class, 5u);
            writer.Write(Sex, 2u);
            writer.Write(GroupId);

            writer.WriteStringWide(GuildName);
            writer.Write(GuildType, 4u);

            writer.Write((byte)GuildIds.Count, 5u);
            GuildIds.ForEach(e => writer.Write(e));
            
            writer.Write(Bones.Count, 6u);
            Bones.ForEach(e => writer.Write(e));

            writer.Write(Title, 14u);
        }
    }
}
