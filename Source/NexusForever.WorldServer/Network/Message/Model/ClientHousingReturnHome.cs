using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientHousingReturnHome)]
    public class ClientHousingReturnHome : IReadable
    {
        public void Read(GamePacketReader reader)
        {
        }
    }
}