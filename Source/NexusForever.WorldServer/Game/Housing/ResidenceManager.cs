using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NexusForever.Database.Character.Model;
using NexusForever.Shared;
using NexusForever.Shared.Database;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;

namespace NexusForever.WorldServer.Game.Housing
{
    public sealed class ResidenceManager : Singleton<ResidenceManager>, IUpdate
    {
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

        private static readonly ConcurrentDictionary</*residenceId*/ ulong, Residence> residences = new ConcurrentDictionary<ulong, Residence>();
        private readonly ConcurrentDictionary</*owner*/ string, ulong /*residenceId*/> ownerCache = new ConcurrentDictionary<string, ulong>(StringComparer.InvariantCultureIgnoreCase);

        private readonly Dictionary<ulong, PublicResidence> visitableResidences = new Dictionary<ulong, PublicResidence>();

        private ImmutableDictionary</*plotIndex*/ byte, PlotPlacement> plotPlacements;
        private readonly Dictionary</*plugId*/ uint, /*residenceInfoId*/uint> residenceLookup = new Dictionary<uint, uint>
            {
                { 80, 11 },     // Cozy Granok House
                { 83, 14 },     // Cozy Aurin House
                { 86, 17 },     // Spacious Exile Human House
                { 294, 18 },    // Cozy Draken House
                { 295, 19 },    // Cozy Chua House
                { 298, 20 },    // Spacious Cassian House
                { 299, 21 },    // Spacious Draken House
                { 293, 22 },    // Cozy Cassian House
                { 296, 23 },    // Spacious Chua House
                { 367, 25 },    // Spaceship (Pre-Order)
                { 297, 26 },    // Spacious Aurin House
                { 291, 27 },    // Spacious Granok House
                { 292, 28 },    // Cozy Exile Human House
                { 530, 32 },    // Underground Bunker
                { 534, 34 },    // Blackhole House
                { 543, 35 },    // Osun House
                { 554, 37 },    // Bird house
                { 557, 38 }     // Royal Piglet
            };
        private readonly Dictionary</*residenceInfoId*/uint, Vector3> residenceTeleportLocation = new Dictionary<uint, Vector3>
            {
                { 11, new Vector3(1484.125f, -895.60f, 1440.239f) },
                { 14, new Vector3(1478.511f, -897.57f, 1444.243f) },
                { 17, new Vector3(1469.454f, -894.02f, 1444.689f) },
                { 18, new Vector3(1483.797f, -822.27f, 1440.55f) },
                { 19, new Vector3(1472.78f, -814.75f, 1444.42f) },
                { 20, new Vector3(1476.702f, -811.31f, 1442.166f) },
                { 21, new Vector3(1486.109f, -851.82f, 1440.203f) },
                { 22, new Vector3(1482.395f, -811.40f, 1444.539f) },
                { 23, new Vector3(1486.433f, -867.77f, 1455.389f) },
                { 25, new Vector3(1491.635f, -903.55f, 1439.926f) },
                { 26, new Vector3(1466.468f, -893f, 1457.137f) },
                { 27, new Vector3(1480.618f, -895.67f, 1425.404f) },
                { 28, new Vector3(1476.236f, -912.67f, 1442.122f) },
                { 32, new Vector3(1497.198f, -912.67f, 1452.01f) },
                { 34, new Vector3(1472f, -903.01f, 1442f) },
                { 35, new Vector3(1530.391f, -969.07f, 1440.467f) },
                { 37, new Vector3(1488.702f, -985.76f, 1440.08f) },
                { 38, new Vector3(1491.635f, -903.55f, 1439.926f) }
            };

        private double timeToSave = SaveDuration;

        private ResidenceManager()
        {
        }

        public void Initialise()
        {
            nextResidenceId = DatabaseManager.Instance.CharacterDatabase.GetNextResidenceId() + 1ul;
            nextDecorId     = DatabaseManager.Instance.CharacterDatabase.GetNextDecorId() + 1ul;

            CachePlotPlacements();

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

        private void CachePlotPlacements()
        {
            var builder = ImmutableDictionary.CreateBuilder<byte, PlotPlacement>();

            builder.Add(0, new PlotPlacement(62612, 35031, new Vector3(1472f, -715.01f, 1440f)));
            builder.Add(1, new PlotPlacement(23863, 30343, new Vector3(1424f, -715.7094f, 1472f)));
            builder.Add(2, new PlotPlacement(23789, 27767, new Vector3(1456f, -714.7094f, 1392f)));
            builder.Add(3, new PlotPlacement(23789, 27767, new Vector3(1488f, -714.7094f, 1392f)));
            builder.Add(4, new PlotPlacement(23789, 27767, new Vector3(1456f, -714.7094f, 1488f)));
            builder.Add(5, new PlotPlacement(23789, 27767, new Vector3(1488f, -714.7094f, 1488f)));
            builder.Add(6, new PlotPlacement(23863, 30343, new Vector3(1424f, -715.7094f, 1408f)));

            plotPlacements = builder.ToImmutable();
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
                return GetCachedResidence(residenceId);

            ResidenceModel model = await DatabaseManager.Instance.CharacterDatabase.GetResidence(name);
            if (model == null)
                return null;

            var residence = new Residence(model);
            residences.TryAdd(residence.Id, residence);
            ownerCache.TryAdd(name, residence.Id);
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

        /// <summary>
        /// Return cached <see cref="Residence"/> by supplied residence id.
        /// </summary>
        public Residence GetCachedResidence(ulong residenceId)
        {
            return residences.TryGetValue(residenceId, out Residence residence) ? residence : null;
        }

        /// <summary>
        /// Get the <see cref="ResidenceEntrance"/> for the provided <see cref="Residence"/>
        /// </summary>
        public WorldLocation2Entry GetResidenceEntranceLocation(Residence residence)
        {
            HousingPropertyInfoEntry propertyEntry = GameTableManager.Instance.HousingPropertyInfo.GetEntry(residence.PropertyInfoId);
            if (propertyEntry == null)
                throw new HousingException();

            return GameTableManager.Instance.WorldLocation2.GetEntry(propertyEntry.WorldLocation2Id);
        }

        /// <summary>
        /// Get the <see cref="ResidenceEntrance"/> for the provided <see cref="Residence"/>
        /// </summary>
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
        /// Returns a <see cref="HousingResidenceInfoEntry"/> ID if the plug ID is known.
        /// </summary>
        public uint GetResidenceEntryForPlug(uint plugItemId)
        {
            return residenceLookup.TryGetValue(plugItemId, out uint residenceId) ? residenceId : 0u;
        }

        /// <summary>
        /// Returns the <see cref="Vector3"/> location for the house inside
        /// </summary>
        public Vector3 GetResidenceInsideLocation(uint residenceInfoId)
        {
            return residenceTeleportLocation.TryGetValue(residenceInfoId, out Vector3 teleportLocation) ? teleportLocation : Vector3.Zero;
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

        /// <summary>
        /// Returns a <see cref="PlotPlacement"/> entry for the given Plot Index
        /// </summary>
        public PlotPlacement GetPlotPlacementInformation(byte index)
        {
            return plotPlacements.TryGetValue(index, out PlotPlacement placementInformation) ? placementInformation : null;
        }
    }
}
