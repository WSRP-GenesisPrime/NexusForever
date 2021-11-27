using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientMovementSpeedSprint)]
    public class ClientMovementSpeedSprint : IReadable
    {
        public bool Sprint { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Sprint = reader.ReadBit();
        }
    }
}