using NexusForever.WorldServer.Game.Entity;

namespace NexusForever.WorldServer.Game.Map
{
    public interface IInstancedMap : IMap
    {
        IMap GetInstance(MapInfo info);
        IMap CreateInstance(MapInfo info, Player player);
    }
}
