using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerResurrectionUpdate)]
    public class ServerResurrectionUpdate : IWritable
    {
        public RezType ShowRezFlags { get; set; } // 8
        public bool Unknown0 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(ShowRezFlags, 8u);
            writer.Write(Unknown0);
        }
    }
}
