using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Spell.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Network.Message.Model.Shared
{
    public class TargetInfo : IWritable // same used for 0x0818
    {
        public uint UnitId { get; set; }
        public byte Ndx { get; set; }
        public byte TargetFlags { get; set; }
        public ushort InstanceCount { get; set; }
        public CombatResult CombatResult { get; set; }

        public List<EffectInfo> EffectInfoData { get; set; } = new List<EffectInfo>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(UnitId);
            writer.Write(Ndx);
            writer.Write(TargetFlags);
            writer.Write(InstanceCount);
            writer.Write(CombatResult, 4u);

            writer.Write(EffectInfoData.Count, 8u);
            EffectInfoData.ForEach(u => u.Write(writer));
        }
    }
}
