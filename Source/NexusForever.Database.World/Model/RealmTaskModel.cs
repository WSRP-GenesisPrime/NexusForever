using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.Database.World.Model
{
    internal class RealmTaskModel
    {
        public uint Id { get; set; }
        public uint Type { get; set; }
        public string Value { get; set; }
        public uint CharacterId { get; set; }
        public uint AccountId { get; set; }
        public uint GuildId { get; set; }
        public uint ReferenceId { get; set; }
        public string ReferenceValue { get; set; }
        public uint Status { get; set; }
        public string StatusDescription { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastRunTime { get; set; }
    }
}
