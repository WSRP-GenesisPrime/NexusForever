using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientResurrectRequest)]
    public class ClientResurrectRequest : IReadable
    {
        public uint UnitId { get; private set; }
        public RezType RezType { get; private set; }

        public void Read(GamePacketReader reader)
        {
            UnitId = reader.ReadUInt();
            RezType = reader.ReadEnum<RezType>(32u);
        }
    }
}
