using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Cinematic.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientLootVacuum)]
    public class ClientLootVacuum : IReadable
    {
        public void Read(GamePacketReader reader)
        {
        }
    }
}
