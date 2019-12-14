using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerPlayerPet)]
    public class ServerPlayerPet : IWritable
    {
        public Pet Pet { get; set; }

        public void Write(GamePacketWriter writer)
        {
            Pet.Write(writer);
        }
    }
}
