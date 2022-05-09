using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientPvpModeSet)]
    public class ClientPvpModeSet : IReadable
    {
        public bool PvpMode { get; private set; }

        public void Read(GamePacketReader reader)
        {
            PvpMode = reader.ReadBit();
        }
    }
}
