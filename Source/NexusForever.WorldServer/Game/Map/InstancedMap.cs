using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NLog;

namespace NexusForever.WorldServer.Game.Map
{
    public sealed class InstancedMap<T> : IInstancedMap where T : IMap, new()
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private WorldEntry entry;
        private readonly Dictionary</*instanceId*/ uint, T> instances = new Dictionary<uint, T>();
        private readonly Queue<T> pendingInstances = new Queue<T>();
        private readonly Queue<uint> closingInstances = new Queue<uint>();
        private readonly QueuedCounter instanceCounter = new QueuedCounter();
        private string type;

        /// <summary>
        /// Unused. Only here to satisfy interface requirements.
        /// </summary>
        public bool IsReadyToUnload() => false;
        
        public void Initialise(MapInfo info, Player player)
        {
            entry = info.Entry;
            type = Regex.Replace(typeof(T).ToString(), @"[.\w]+\.(\w+)", "$1");
        }

        public void EnqueueAdd(GridEntity entity, Vector3 position)
        {
            // entities are never directly added to an instanced map, only its children
            throw new InvalidOperationException();
        }

        public void Update(double lastTick)
        {
            var sw = Stopwatch.StartNew();

            while (pendingInstances.TryDequeue(out T instance))
                instances.Add(instanceCounter.Dequeue(), instance);

            foreach ((uint instanceId, T map) in instances)
            {
                map.Update(lastTick);
                if (map.IsReadyToUnload())
                    closingInstances.Enqueue(instanceId);
            }

            while (closingInstances.TryDequeue(out uint instanceId))
            {
                instances.Remove(instanceId);
                instanceCounter.Enqueue(instanceId);
            }

            sw.Stop();
            if (sw.ElapsedMilliseconds > 1)
                log.Trace($"{instances.Count} Instanced {type}(s) took {sw.ElapsedMilliseconds}ms to update!");
        }

        /// <summary>
        /// Create or find child <see cref="T"/> for <see cref="Player"/>.
        /// </summary>
        public IMap CreateInstance(MapInfo info, Player player)
        {
            if (info.InstanceId != 0u)
                return GetInstance(info.InstanceId);
            if (info.ResidenceId != 0ul)
            {
                T instance = GetInstance(i => (i as ResidenceMap)?.Id == info.ResidenceId);
                if (instance != null)
                    return instance;
            }

            T newInstance = new T();
            newInstance.Initialise(info, player);
            pendingInstances.Enqueue(newInstance);
            return newInstance;
        }

        private T GetInstance(uint instanceId)
        {
            return instances.TryGetValue(instanceId, out T map) ? map : default;
        }

        private T GetInstance(Func<T, bool> func)
        {
            foreach ((uint instanceId, T map) in instances)
                if (func.Invoke(map))
                    return map;

            return default;
        }
    }
}
