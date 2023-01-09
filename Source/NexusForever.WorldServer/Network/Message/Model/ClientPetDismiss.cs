using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientPetDismiss)]
    public class ClientPetDismiss : IReadable
    {
        public uint Unknown0 { get; private set; }
        public uint Unknown1 { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Unknown0 = reader.ReadUInt();
            Unknown1 = reader.ReadUInt();
        }
    }
}
