using NexusForever.Shared.GameTable;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Reputation.Static;
using System.Collections.Generic;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Creature.Transporters
{
    [Script(35298)] // Housing - Return Teleporter
    public class HousingTeleporter : CreatureScript
    {
        private Dictionary<Faction, MapPosition> cityLocations = new Dictionary<Faction, MapPosition>
        {
            {  Faction.Dominion, new MapPosition
                {
                    Info = new MapInfo
                    {
                        Entry = GameTableManager.Instance.World.GetEntry(22)
                    },
                    Position = new Vector3(-3257.656f, -905.20825f, -819.9719f)
                }
            },
            { Faction.Exile, new MapPosition
                {
                    Info = new MapInfo
                    {
                        Entry = GameTableManager.Instance.World.GetEntry(51)
                    },
                    Position = new Vector3(4040.8342f, -821.95105f, -1706.8625f)
                }
            }
        };

        public override void OnCreate(WorldEntity me)
        {
            me.RangeCheck = 2f;
        }

        public override void OnEnterRange(WorldEntity me, WorldEntity activator)
        {
            if (activator is not Player player)
                return;

            if (!player.CanTeleport())
                return;

            if (me.Position.Y - 1f > activator.Position.Y)
                return;

            if (cityLocations.TryGetValue(player.Faction, out MapPosition destination))
                player.TeleportTo(destination);
        }
    }
}
