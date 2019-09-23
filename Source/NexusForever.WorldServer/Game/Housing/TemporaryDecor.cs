using NexusForever.Shared.GameTable.Model;
using System.Numerics;

namespace NexusForever.WorldServer.Game.Housing
{
    public class TemporaryDecor : Decor
    {
        public new long DecorId;

        public TemporaryDecor(ulong id, long decorId, HousingDecorInfoEntry entry, Vector3 position, Quaternion rotation)
        {
            Id = id;
            DecorId = decorId;
            Entry = entry;
            Position = position;
            Rotation = rotation;
        }
    }
}
