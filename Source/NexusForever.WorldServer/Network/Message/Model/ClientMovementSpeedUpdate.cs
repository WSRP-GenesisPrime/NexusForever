using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientMovementSpeedUpdate)]
    public class ClientMovementSpeedUpdate : IReadable
    {
        public MovementSpeed Speed { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Speed = reader.ReadEnum<MovementSpeed>(32u);
        }
    }
}