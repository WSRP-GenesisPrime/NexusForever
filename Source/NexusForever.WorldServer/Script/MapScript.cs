using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script
{
    public abstract class MapScript : Script
    {
        public virtual void OnAddToMap(Player player)
        {
        }

        public virtual void OnEnterZone(Player player, uint zoneId)
        {
        }
    }
}
