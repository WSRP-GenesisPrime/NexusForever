using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerHousingNeighbors)]
    public class ServerHousingNeighbors : IWritable
    {
        public class NeighborData : IWritable
        {
            public ulong ContactID1 { get; set; }
            public ulong ContactID2 { get; set; }
            public ushort Unknown1 { get; set; } = 0;
            public ulong CharacterID { get; set; }
            public uint Unknown2 { get; set; } = 8;

            public void Write(GamePacketWriter writer)
            {
                writer.Write(ContactID1);
                writer.Write(ContactID2);
                writer.Write(Unknown1);
                writer.Write(CharacterID);
                writer.Write(Unknown2); // can be 3 or 4 bytes when its value is 0; otherwise always 4 bytes.
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
