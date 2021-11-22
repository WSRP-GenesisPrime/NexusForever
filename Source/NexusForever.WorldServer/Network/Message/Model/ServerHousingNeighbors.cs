using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerHousingNeighbors)]
    public class ServerHousingNeighbors : IWritable
    {
        public static ServerHousingNeighbors dummy()
        {
            ServerHousingNeighbors shn = new ServerHousingNeighbors();
            NeighborData nd = new NeighborData();
            nd.NeighborId = 1;
            nd.NeighborHoodId = 2;
            nd.RealmId = WorldServer.RealmId;
            nd.CharacterID = 352;
            shn.Neighbors.Add(nd);
            return shn;
        }

        public class NeighborData : IWritable
        {
            public ulong NeighborId { get; set; }
            public ulong NeighborHoodId { get; set; }
            public ushort RealmId { get; set; } = 0;
            public ulong CharacterID { get; set; }
            public uint PermissionLevel { get; set; } = 2;

            public void Write(GamePacketWriter writer)
            {
                writer.Write(NeighborId, 64u);
                writer.Write(NeighborHoodId, 64u);
                writer.Write(RealmId, 14u);
                writer.Write(CharacterID, 64u);
                writer.Write(PermissionLevel, 32u);
            }
        }

        public List<NeighborData> Neighbors { get; set; } = new List<NeighborData>();
        public void Write(GamePacketWriter writer)
        {
            writer.Write((uint) Neighbors.Count);
            foreach(var neighbor in Neighbors)
            {
                neighbor.Write(writer);
            }
        }
    }
}
