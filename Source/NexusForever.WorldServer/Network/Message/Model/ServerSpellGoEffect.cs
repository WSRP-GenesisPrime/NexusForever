using System.Collections.Generic;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerSpellGoEffect)]
    public class ServerSpellGoEffect : IWritable
    {
        public uint ServerUniqueId { get; set; }
        public uint Spell4EffectId { get; set; }
        public uint TargetId { get; set; }

        public List<DamageDescription> DamageDescriptionData { get; set; } = new();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(ServerUniqueId);
            writer.Write(Spell4EffectId, 19u);
            writer.Write(TargetId);

            writer.Write(DamageDescriptionData.Count, 8u);
            DamageDescriptionData.ForEach(u => u.Write(writer));
        }
    }
}
