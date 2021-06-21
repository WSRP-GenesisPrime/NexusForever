﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexusForever.Database.Character.Model;
using NexusForever.Shared;
using NexusForever.Shared.Database;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NLog;

namespace NexusForever.WorldServer.Game.Housing
{
    public sealed class ResidenceManager : Singleton<ResidenceManager>, IUpdate
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        // TODO: move this to the config file
        private const double SaveDuration = 60d;

        /// <summary>
        /// Id to be assigned to the next created residence.
        /// </summary>
        public ulong NextResidenceId => nextResidenceId++;

        /// <summary>
        /// Id to be assigned to the next created residence.
        /// </summary>
        public ulong NextDecorId => nextDecorId++;

        private ulong nextResidenceId;
        private ulong nextDecorId;

        private static readonly ConcurrentDictionary</*residenceId*/ ulong, Residence> residences = new();
        private readonly ConcurrentDictionary</*owner*/ string, ulong /*residenceId*/> ownerCache = new(StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<ulong, PublicResidence> visitableResidences = new();

        private double timeToSave = SaveDuration;

        private ResidenceManager()
        {
        }

        public void Initialise()
        {
            nextResidenceId = DatabaseManager.Instance.CharacterDatabase.GetNextResidenceId() + 1ul;
            nextDecorId     = DatabaseManager.Instance.CharacterDatabase.GetNextDecorId() + 1ul;

            foreach (ResidenceModel residence in DatabaseManager.Instance.CharacterDatabase.GetPublicResidences())
                RegisterResidenceVists(residence.Id, residence.Character.Name, residence.Name);
        }

        public void Update(double lastTick)
        {
            timeToSave -= lastTick;
            if (timeToSave <= 0d)
            {
                var tasks = new List<Task>();
                foreach (Residence residence in residences.Values)
                    tasks.Add(DatabaseManager.Instance.CharacterDatabase.Save(residence.Save));

                Task.WaitAll(tasks.ToArray());

                timeToSave = SaveDuration;
            }
        }

        /// <summary>
        /// Create new <see cref="Residence"/> for supplied <see cref="Player"/>.
        /// </summary>
        public Residence CreateResidence(Player player)
        {
            var residence = new Residence(player);
            residences.TryAdd(residence.Id, residence);
            ownerCache.TryAdd(player.Name, residence.Id);
            return residence;
        }

        /// <summary>
        /// Return existing <see cref="Residence"/> by supplied residence id, if not locally cached it will be retrieved from the database.
        /// </summary>
        public async Task<Residence> GetResidence(ulong residenceId)
        {
            Residence residence = GetCachedResidence(residenceId);
            if (residence != null)
                return residence;

            ResidenceModel model = await DatabaseManager.Instance.CharacterDatabase.GetResidence(residenceId);
            if (model == null)
                return null;

            residence = new Residence(model);
            residences.TryAdd(residence.Id, residence);
            ownerCache.TryAdd(model.Character.Name, residence.Id);
            return residence;
        }

        /// <summary>
        /// Return existing <see cref="Residence"/> by supplied owner name, if not locally cached it will be retrieved from the database.
        /// </summary>
        public async Task<Residence> GetResidence(string name)
        {
            if (ownerCache.TryGetValue(name, out ulong residenceId))
            {
                Residence r = GetCachedResidence(residenceId);
                log.Trace($"Getting residence from cache: {residenceId}, {r.Id}, {r.OwnerId}, for name {name}.");
                return r;
            }

            ResidenceModel model = await DatabaseManager.Instance.CharacterDatabase.GetResidence(name);
            if (model == null)
            {
                log.Trace($"Found no residence for name {name}, returning null.");
                return null;
            }

            var residence = new Residence(model);
            residences.TryAdd(residence.Id, residence);
            ownerCache.TryAdd(name, residence.Id);
            log.Trace($"Getting residence from database: {residence.Id}, {residence.OwnerId}, for name {name}.");
            return residence;
        }

        /// <summary>
        /// Remove an existing <see cref="Residence"/> from the 
        /// </summary>
        public void RemoveResidence(string name)
        {
            if (ownerCache.TryRemove(name, out ulong residenceId))
                DeregisterResidenceVists(residenceId);

            // TODO: Kick any players out of the ResidenceMap and close the Instance.
        }

        public void UnloadResidence(string name)
        {
            if (ownerCache.TryGetValue(name, out ulong residenceId))
            {
                residences.TryRemove(residenceId, out _);
                ownerCache.TryRemove(name, out _);
            }
        }

        /// <summary>
        /// Return cached <see cref="Residence"/> by supplied residence id.
        /// </summary>
        public Residence GetCachedResidence(ulong residenceId)
        {
            return residences.TryGetValue(residenceId, out Residence residence) ? residence : null;
        }

        public ResidenceEntrance GetResidenceEntrance(Residence residence)
        {
            HousingPropertyInfoEntry propertyEntry = GameTableManager.Instance.HousingPropertyInfo.GetEntry(residence.PropertyInfoId);
            if (propertyEntry == null)
                throw new HousingException();

            WorldLocation2Entry locationEntry = GameTableManager.Instance.WorldLocation2.GetEntry(propertyEntry.WorldLocation2Id);
            return new ResidenceEntrance(locationEntry);
        }

        /// <summary>
        /// Register residence as visitable, this allows anyone to visit through the random property feature.
        /// </summary>
        public void RegisterResidenceVists(ulong residenceId, string owner, string name)
        {
            visitableResidences.Add(residenceId, new PublicResidence(residenceId, owner, name));
        }

        /// <summary>
        /// Deregister residence as visitable, this prevents anyone from visiting through the random property feature.
        /// </summary>
        public void DeregisterResidenceVists(ulong residenceId)
        {
            visitableResidences.Remove(residenceId);
        }

        /// <summary>
        /// Return 50 random registered visitable residences.
        /// </summary>
        public IEnumerable<PublicResidence> GetRandomVisitableResidences()
        {
            var random = new Random();
            return visitableResidences
                .Values
                .OrderBy(r => random.Next())
                .Take(50);
        }
    }
}
