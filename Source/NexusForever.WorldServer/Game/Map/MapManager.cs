using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using NexusForever.Shared;
using NexusForever.WorldServer.Game.Entity;
using NLog;

namespace NexusForever.WorldServer.Game.Map
{
    public sealed class MapManager : Singleton<MapManager>, IUpdate
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary</*worldId*/ ushort, IMap> maps = new();

        private MapManager()
        {
        }

        public void Update(double lastTick)
        {
            if (maps.Count == 0)
                return;

            var sw = Stopwatch.StartNew();

            var tasks = new List<Task>();
            foreach (IMap map in maps.Values)
                tasks.Add(Task.Run(() => { map.Update(lastTick); }));
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch
            {
                // ignored.
            }

            sw.Stop();
            if (sw.ElapsedMilliseconds > 10)
                log.Warn($"{maps.Count} map(s) took {sw.ElapsedMilliseconds}ms to update!");
        }

        /// <summary>
        /// Enqueue <see cref="Player"/> to be added to a map. 
        /// </summary>
        public void AddToMap(Player player, MapInfo info, Vector3 vector3)
        {
            if (info?.Entry == null)
                throw new ArgumentException();

            IMap map = CreateMap(info, player);
            map.EnqueueAdd(player, vector3);
        }

        public IMap GetMap(MapInfo info)
        {
            IMap map = GetBaseMap(info);
                if (map is IInstancedMap iMap)
                    map = iMap.GetInstance(info);

            return map;
        }

        /// <summary>
        /// Create base or instanced <see cref="IMap"/> of <see cref="MapInfo"/> for <see cref="Player"/>.
        /// </summary>
        private IMap CreateMap(MapInfo info, Player player)
        {
            IMap map = CreateBaseMap(info);
            if (map is IInstancedMap iMap)
                map = iMap.CreateInstance(info, player);

            return map;
        }

        private IMap GetBaseMap(MapInfo info)
        {
            if (maps.TryGetValue((ushort)info.Entry.Id, out IMap map))
            {
                log.Trace($"MapManager: Loading existing map {info.Entry.Id}");
                return map;
            }
            return null;
        }

        /// <summary>
        /// Create and store base <see cref="IMap"/> of <see cref="MapInfo"/>.
        /// </summary>
        private IMap CreateBaseMap(MapInfo info)
        {
            IMap map = GetBaseMap(info);
            if(map != null)
            {
                return map;
            }

            log.Trace($"MapManager: Creating map instance {info.Entry.Id}");

            switch (info.Entry.Type)
            {
                case 5:
                    map = new InstancedMap<ResidenceMap>();
                    break;
                default:
                    map = new BaseMap();
                    break;
            }
            
            map.Initialise(info, null);
            maps.Add((ushort)info.Entry.Id, map);
            return map;
        }
    }
}
