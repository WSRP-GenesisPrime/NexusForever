using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Contact.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientHousingNeighborUpdate)]
    public class ClientHousingNeighborUpdate : IReadable
    {
        public TargetPlayerIdentity PlayerIdentity { get; private set; } = new TargetPlayerIdentity();
        public string Comment { get; private set; }
        public uint Permissions { get; private set; }

        public void Read(GamePacketReader reader)
        {
            PlayerIdentity.Read(reader);
            Comment = reader.ReadWideString();
            Permissions = reader.ReadUInt();
        }
    }
}
