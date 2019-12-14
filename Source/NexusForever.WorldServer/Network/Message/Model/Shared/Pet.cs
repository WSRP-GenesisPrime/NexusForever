using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class Pet : IWritable
    {
        public uint Guid { get; set; }
        public uint SummoningSpell { get; set; }
        public byte ValidStances { get; set; }
        public byte Stance { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Guid);
            writer.Write(SummoningSpell, 18u);
            writer.Write(ValidStances, 5u);
            writer.Write(Stance, 5u);
        }
    }
}
