using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientIcCommSendMessage)]
    public class ClientIcCommSendMessage : IReadable
    {

        public void Read(GamePacketReader reader)
        {
            string ChannelName   = reader.ReadWideString();
        }
    }
}
