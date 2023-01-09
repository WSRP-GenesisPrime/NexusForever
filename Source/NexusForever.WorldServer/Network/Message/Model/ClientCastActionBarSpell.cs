using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientCastActionBarSpell)]
    public class ClientCastActionBarSpell : IReadable
    {
        public uint ClientUniqueId { get; private set; }
        public byte ActionBarSetIndex { get; private set; } // 4
        public ShortcutSet WhichSet { get; private set; } // 4
        public uint TargetUnitId { get; private set; }
        public Position TargetPosition { get; private set; } = new();

        public void Read(GamePacketReader reader)
        {
            ClientUniqueId      = reader.ReadUInt();
            ActionBarSetIndex   = reader.ReadByte(4u);
            WhichSet            = reader.ReadEnum<ShortcutSet>(4u);
            TargetUnitId        = reader.ReadUInt();

            TargetPosition.Read(reader);
        }
    }
}
