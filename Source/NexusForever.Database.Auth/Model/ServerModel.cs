﻿namespace NexusForever.Database.Auth.Model
{
    public class ServerModel
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public string InternalIP { get; set; }
        public bool AssumeOnline { get; set; }
        public ushort Port { get; set; }
        public byte Type { get; set; }
    }
}
