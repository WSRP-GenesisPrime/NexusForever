using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Contact.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientHousingNeighborAdd)]
    public class ClientHousingNeighborAdd : IReadable
    {
        public string PlayerName { get; private set; }

        public void Read(GamePacketReader reader)
        {
            reader.ReadULong();
            reader.ReadULong(14);
            PlayerName  = reader.ReadWideString();
        }
    }
}
