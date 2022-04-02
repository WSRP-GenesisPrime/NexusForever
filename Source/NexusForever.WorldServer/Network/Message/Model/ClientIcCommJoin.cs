using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientIcCommJoin)]
    public class ClientIcCommJoin : IReadable
    {

        public void Read(GamePacketReader reader)
        {
            ulong ChannelId      = reader.ReadULong(48);
            reader.ReadULong(19);
            string ChannelName   = reader.ReadWideString();
        }
    }
}
