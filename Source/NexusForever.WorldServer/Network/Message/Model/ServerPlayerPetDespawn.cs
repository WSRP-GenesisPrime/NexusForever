using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerPlayerPetDespawn)]
    public class ServerPlayerPetDespawn : IWritable
    {
        public uint Guid { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Guid);
        }
    }
}
