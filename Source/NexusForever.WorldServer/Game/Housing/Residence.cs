using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Housing.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Map.Search;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Game.Housing
{
    public class Residence : ISaveCharacter
    {
        public ulong Id { get; }
        public ulong OwnerId { get; }
        public string OwnerName { get; }
        public string OwnerOriginalName { get; }
        public byte PropertyInfoId { get; }

        private bool has18PlusLock = false;
        private DateTime unlockTime18Plus = DateTime.MinValue;
        private bool waitForEmptyPlot18Plus = false;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                saveMask |= ResidenceSaveMask.Name;
            }
        }

        private string name;

        public ResidencePrivacyLevel PrivacyLevel
        {
            get => privacyLevel;
            set
            {
                if (value > ResidencePrivacyLevel.Private)
                    throw new ArgumentOutOfRangeException();

                privacyLevel = value;
                saveMask |= ResidenceSaveMask.PrivacyLevel;
            }
        }

        private ResidencePrivacyLevel privacyLevel;

        public ushort Wallpaper
        {
            get => wallpaperId;
            set
            {
                if (GameTableManager.Instance.HousingWallpaperInfo.GetEntry(value) == null && value > 0)
                    throw new ArgumentOutOfRangeException();

                wallpaperId = value;
                saveMask |= ResidenceSaveMask.Wallpaper;
            }
        }

        private ushort wallpaperId;

        public ushort Roof
        {
            get => roofDecorInfoId;
            set
            {
                if (GameTableManager.Instance.HousingDecorInfo.GetEntry(value) == null && value > 0)
                    throw new ArgumentOutOfRangeException();

                roofDecorInfoId = value;
                saveMask |= ResidenceSaveMask.Roof;
            }
        }

        private ushort roofDecorInfoId;

        public ushort Entryway
        {
            get => entrywayDecorInfoId;
            set
            {
                if (GameTableManager.Instance.HousingDecorInfo.GetEntry(value) == null && value > 0)
                    throw new ArgumentOutOfRangeException();

                entrywayDecorInfoId = value;
                saveMask |= ResidenceSaveMask.Entryway;
            }
        }

        private ushort entrywayDecorInfoId;

        public ushort Door
        {
            get => doorDecorInfoId;
            set
            {
                if (GameTableManager.Instance.HousingDecorInfo.GetEntry(value) == null && value > 0)
                    throw new ArgumentOutOfRangeException();

                doorDecorInfoId = value;
                saveMask |= ResidenceSaveMask.Door;
            }
        }

        private ushort doorDecorInfoId;

        public ushort Music
        {
            get => musicId;
            set
            {
                HousingWallpaperInfoEntry entry = GameTableManager.Instance.HousingWallpaperInfo.GetEntry(value);
                if (entry == null)
                    throw new ArgumentOutOfRangeException();

                if ((entry.Flags & 0x100) == 0)
                    throw new ArgumentOutOfRangeException();

                musicId = value;
                saveMask |= ResidenceSaveMask.Music;
            }
        }

        private ushort musicId;

        public ushort Ground
        {
            get => groundWallpaperId;
            set
            {
                HousingWallpaperInfoEntry entry = GameTableManager.Instance.HousingWallpaperInfo.GetEntry(value);
                if (entry == null)
                    throw new ArgumentOutOfRangeException();

                if ((entry.Flags & 0x200) == 0)
                    throw new ArgumentOutOfRangeException();

                groundWallpaperId = value;
                saveMask |= ResidenceSaveMask.Ground;
            }
        }

        private ushort groundWallpaperId;

        public ushort Sky
        {
            get => skyWallpaperId;
            set
            {
                HousingWallpaperInfoEntry entry = GameTableManager.Instance.HousingWallpaperInfo.GetEntry(value);
                if (entry == null)
                    throw new ArgumentOutOfRangeException();

                if ((entry.Flags & 0x40) == 0)
                    throw new ArgumentOutOfRangeException();

                skyWallpaperId = value;
                saveMask |= ResidenceSaveMask.Sky;
            }
        }

        private ushort skyWallpaperId;

        public ResidenceFlags Flags
        {
            get => flags;
            set
            {
                flags = value;
                saveMask |= ResidenceSaveMask.Flags;
            }
        }

        private ResidenceFlags flags;

        public byte ResourceSharing
        {
            get => resourceSharing;
            set
            {
                resourceSharing = value;
                saveMask |= ResidenceSaveMask.ResourceSharing;
            }
        }

        private byte resourceSharing;

        public byte GardenSharing
        {
            get => gardenSharing;
            set
            {
                gardenSharing = value;
                saveMask |= ResidenceSaveMask.GardenSharing;
            }
        }

        private byte gardenSharing;
        public HousingResidenceInfoEntry ResidenceInfoEntry { get; private set; }

        private ResidenceSaveMask saveMask;

        private readonly Dictionary<ulong, Decor> decors = new();
        private readonly HashSet<Decor> deletedDecors = new();
        private readonly Plot[] plots = new Plot[7];

        /// <summary>
        /// Create a new <see cref="Residence"/> from an existing database model.
        /// </summary>
        public Residence(ResidenceModel model)
        {
            Id                  = model.Id;
            OwnerId             = model.OwnerId;
            OwnerName           = model.Character.Name;
            OwnerOriginalName   = model.Character.OriginalName;
            PropertyInfoId      = model.PropertyInfoId;
            name                = model.Name;
            privacyLevel        = (ResidencePrivacyLevel)model.PrivacyLevel;
            wallpaperId         = model.WallpaperId;
            roofDecorInfoId     = model.RoofDecorInfoId;
            entrywayDecorInfoId = model.EntrywayDecorInfoId;
            doorDecorInfoId     = model.DoorDecorInfoId;
            groundWallpaperId   = model.GroundWallpaperId;
            musicId             = model.MusicId;
            skyWallpaperId      = model.SkyWallpaperId;
            flags               = (ResidenceFlags)model.Flags;
            resourceSharing     = model.ResourceSharing;
            gardenSharing       = model.GardenSharing;
            has18PlusLock       = model.NSFWLock;

            if (model.ResidenceInfoId > 0)
                ResidenceInfoEntry = GameTableManager.Instance.HousingResidenceInfo.GetEntry(model.ResidenceInfoId);

            foreach (ResidenceDecor decorModel in model.Decor)
            {
                var decor = new Decor(decorModel);
                decors.Add(decor.DecorId, decor);
            }

            foreach (ResidencePlotModel plotModel in model.Plot)
            {
                var plot = new Plot(plotModel);
                plots[plot.Index] = plot;
            }

            saveMask = ResidenceSaveMask.None;
        }

        /// <summary>
        /// Create a new <see cref="Residence"/> from a <see cref="Player"/>.
        /// </summary>
        public Residence(Player player)
        {
            Id             = ResidenceManager.Instance.NextResidenceId;
            OwnerId        = player.CharacterId;
            OwnerName      = player.Name;
            PropertyInfoId = 35; // TODO: 35 is default for single residence, this will need to change for communities
            name           = $"{player.Name}'s House";
            privacyLevel   = ResidencePrivacyLevel.Public;

            IEnumerable<HousingPlotInfoEntry> plotEntries = GameTableManager.Instance.HousingPlotInfo.Entries.Where(e => e.HousingPropertyInfoId == PropertyInfoId);
            foreach (HousingPlotInfoEntry entry in plotEntries)
            {
                var plot = new Plot(Id, entry);
                plots[plot.Index] = plot;
            }

            // TODO: find a better way to do this, this adds the starter tent plug
            plots[0].SetPlug(18);
            plots[0].BuildState = 4;

            saveMask = ResidenceSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask != ResidenceSaveMask.None)
            {
                if ((saveMask & ResidenceSaveMask.Create) != 0)
                {
                    // residence doesn't exist in database, all infomation must be saved
                    context.Add(new ResidenceModel
                    {
                        Id                  = Id,
                        OwnerId             = OwnerId,
                        PropertyInfoId      = PropertyInfoId,
                        Name                = Name,
                        PrivacyLevel        = (byte)privacyLevel,
                        WallpaperId         = wallpaperId,
                        RoofDecorInfoId     = roofDecorInfoId,
                        EntrywayDecorInfoId = entrywayDecorInfoId,
                        DoorDecorInfoId     = doorDecorInfoId,
                        GroundWallpaperId   = groundWallpaperId,
                        MusicId             = musicId,
                        SkyWallpaperId      = skyWallpaperId,
                        Flags               = (ushort)flags,
                        ResourceSharing     = resourceSharing,
                        GardenSharing       = gardenSharing
                    });
                }
                else
                {
                    // residence already exists in database, save only data that has been modified
                    var model = new ResidenceModel
                    {
                        Id             = Id,
                        OwnerId        = OwnerId,
                        PropertyInfoId = PropertyInfoId
                    };

                    // could probably clean this up with reflection, works for the time being
                    EntityEntry<ResidenceModel> entity = context.Attach(model);
                    if ((saveMask & ResidenceSaveMask.Name) != 0)
                    {
                        model.Name = Name;
                        entity.Property(p => p.Name).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.PrivacyLevel) != 0)
                    {
                        model.PrivacyLevel = (byte)PrivacyLevel;
                        entity.Property(p => p.PrivacyLevel).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Wallpaper) != 0)
                    {
                        model.WallpaperId = Wallpaper;
                        entity.Property(p => p.WallpaperId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Roof) != 0)
                    {
                        model.RoofDecorInfoId = Roof;
                        entity.Property(p => p.RoofDecorInfoId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Entryway) != 0)
                    {
                        model.EntrywayDecorInfoId = Entryway;
                        entity.Property(p => p.EntrywayDecorInfoId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Door) != 0)
                    {
                        model.DoorDecorInfoId = Door;
                        entity.Property(p => p.DoorDecorInfoId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Ground) != 0)
                    {
                        model.GroundWallpaperId = Ground;
                        entity.Property(p => p.GroundWallpaperId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Music) != 0)
                    {
                        model.MusicId = Music;
                        entity.Property(p => p.MusicId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Sky) != 0)
                    {
                        model.SkyWallpaperId = Sky;
                        entity.Property(p => p.SkyWallpaperId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.Flags) != 0)
                    {
                        model.Flags = (ushort)Flags;
                        entity.Property(p => p.Flags).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.ResourceSharing) != 0)
                    {
                        model.ResourceSharing = ResourceSharing;
                        entity.Property(p => p.ResourceSharing).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.ResidenceInfo) != 0)
                    {
                        model.ResidenceInfoId = (ushort)(ResidenceInfoEntry?.Id ?? 0u);
                        entity.Property(p => p.ResidenceInfoId).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.GardenSharing) != 0)
                    {
                        model.GardenSharing = GardenSharing;
                        entity.Property(p => p.GardenSharing).IsModified = true;
                    }
                    if ((saveMask & ResidenceSaveMask.NSFWLock) != 0)
                    {
                        model.NSFWLock = has18PlusLock;
                        entity.Property(p => p.NSFWLock).IsModified = true;
                    }
                }

                saveMask = ResidenceSaveMask.None;
            }

            foreach (Decor decor in decors.Values)
                decor.Save(context);

            foreach (Decor decor in deletedDecors.ToList())
                decor.Save(context);
            deletedDecors.Clear();

            foreach (Plot plot in plots)
                plot.Save(context);
        }
        public bool Has18PlusLock()
        {
            if (has18PlusLock)
            {
                if (unlockTime18Plus < DateTime.Now)
                {
                    /*if (waitForEmptyPlot18Plus)
                    {
                        return true; // will trigger when last person leaves
                    }
                    else
                    {*/
                        Set18PlusLockInternal(false);
                        return false;
                    //}
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        private void Set18PlusLockInternal(bool value)
        {
            if (has18PlusLock != value)
            {
                saveMask |= ResidenceSaveMask.NSFWLock;
                has18PlusLock = value;
            }
        }

        private bool Can18PlusLock(ResidenceMapInstance map)
        {
            if(map == null)
            {
                return true;
            }
            map.Search(Vector3.Zero, -1, new SearchCheckRangePlayerOnly(Vector3.Zero, -1), out List<GridEntity> entities);
            foreach (GridEntity entity in entities)
            {
                Player player = entity as Player;
                if (player != null && !player.IsAdult)
                {
                    return false; // can't lock, kiddies on the plot.
                }
            }
            return true;
        }

        public ResidenceMapInstance getMap()
        {
            ResidenceEntrance entrance = ResidenceManager.Instance.GetResidenceEntrance(this);
            MapInfo mi = new MapInfo
            {
                Entry = entrance.Entry,
                InstanceId = Id
            };
            ResidenceMapInstance map = MapManager.Instance.GetMap(mi) as ResidenceMapInstance;
            return map;
        }

        public bool Set18PlusLock(bool doLock, DateTime? limit = null, string timeText = null)
        {
            if(doLock == has18PlusLock && limit == null)
            {
                return true;
            }
            ResidenceMapInstance map = getMap();
            if(map == null)
            {
                Set18PlusLockInternal(doLock);
                return true;
            }
            if (doLock && Can18PlusLock(map))
            {
                string text = "18+ lock created.";
                if (!string.IsNullOrWhiteSpace(timeText))
                {
                    text = $"18+ lock created, and will last for {timeText}.";
                }
                map.EnqueueToAll(new ServerChat
                {
                    Channel = new Channel
                    {
                        Type = ChatChannelType.System
                    },
                    Text = text
                });
                Set18PlusLockInternal(doLock);
                if(limit != null)
                {
                    set18PlusTimeLimit(limit);
                }
                else
                {
                    set18PlusTimeLimit(DateTime.MaxValue);
                }
                return true;
            }
            if (!doLock)
            {
                map.EnqueueToAll(new ServerChat
                {
                    Channel = new Channel
                    {
                        Type = ChatChannelType.System
                    },
                    Text = "18+ lock dropped."
                });
                Set18PlusLockInternal(doLock);
                return true;
            }
            return false;
        }

        public void set18PlusTimeLimit(DateTime? limit)
        {
            if (!has18PlusLock)
            {
                throw new InvalidOperationException();
            }
            DateTime val = unlockTime18Plus;
            if (limit == null)
            {
                unlockTime18Plus = DateTime.MinValue;
            }
            else
            {
                unlockTime18Plus = (DateTime) limit;
            }
            if(unlockTime18Plus != val)
            {
                saveMask |= ResidenceSaveMask.NSFWLock;
            }
        }

        public void set18PlusWaitForEmpty(bool wait)
        {
            if(!has18PlusLock)
            {
                throw new InvalidOperationException();
            }
            if (waitForEmptyPlot18Plus != wait)
            {
                waitForEmptyPlot18Plus = wait;
                saveMask |= ResidenceSaveMask.NSFWLock;
            }
        }

        /// <summary>
        /// Returns true if the supplied character id can modify the <see cref="Residence"/>.
        /// </summary>
        public bool CanModifyResidence(ulong characterId)
        {
            // TODO: roommates can also update decor
            return characterId == OwnerId;
        }

        /// <summary>
        /// Return all <see cref="Plot"/>'s for the <see cref="Residence"/>.
        /// </summary>
        public IEnumerable<Plot> GetPlots()
        {
            return plots;
        }

        /// <summary>
        /// Return all <see cref="Decor"/> for the <see cref="Residence"/>.
        /// </summary>
        public IEnumerable<Decor> GetDecor()
        {
            return decors.Values;
        }

        /// <summary>
        /// Return all <see cref="Decor"/> placed in the world for the <see cref="Residence"/>.
        /// </summary>
        public IEnumerable<Decor> GetPlacedDecor()
        {
            foreach (Decor decor in decors.Values)
                if (decor.Type != DecorType.Crate)
                    yield return decor;
        }

        /// <summary>
        /// Return <see cref="Decor"/> with the supplied id.
        /// </summary>
        public Decor GetDecor(ulong decorId)
        {
            decors.TryGetValue(decorId, out Decor decor);
            return decor;
        }

        public Decor DecorCreate(HousingDecorInfoEntry entry)
        {
            var decor = new Decor(Id, ResidenceManager.Instance.NextDecorId, entry);
            decors.Add(decor.DecorId, decor);
            return decor;
        }

        public void DecorDelete(Decor decor)
        {
            decor.EnqueueDelete();

            decors.Remove(decor.DecorId);
            deletedDecors.Add(decor);
        }


        /// <summary>
        /// Set this <see cref="Residence"/> house plug to the supplied <see cref="HousingPlugItemEntry"/>. Returns <see cref="true"/> if successful
        /// </summary>
        public bool SetHouse(HousingPlugItemEntry plugItemEntry)
        {
            if (plugItemEntry == null)
                throw new ArgumentNullException();

            uint residenceId = GetResidenceEntryForPlug(plugItemEntry.Id);
            if (residenceId > 0)
            {
                HousingResidenceInfoEntry residenceInfoEntry = GameTableManager.Instance.HousingResidenceInfo.GetEntry(residenceId);
                if (residenceInfoEntry != null)
                {
                    ResidenceInfoEntry = residenceInfoEntry;
                    Wallpaper = (ushort)residenceInfoEntry.HousingWallpaperInfoIdDefault;
                    Roof = (ushort)residenceInfoEntry.HousingDecorInfoIdDefaultRoof;
                    Door = (ushort)residenceInfoEntry.HousingDecorInfoIdDefaultDoor;
                    Entryway = (ushort)residenceInfoEntry.HousingDecorInfoIdDefaultEntryway;
                }

                saveMask |= ResidenceSaveMask.ResidenceInfo;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a <see cref="HousingResidenceInfoEntry"/> ID if the plug ID is known.
        /// </summary>
        private uint GetResidenceEntryForPlug(uint plugItemId)
        {
            Dictionary<uint, uint> residenceLookup = new Dictionary<uint, uint>
            {
                { 83, 14 },     // Cozy Aurin House
                { 295, 19 },    // Cozy Chua House
                { 293, 22 },    // Cozy Cassian House
                { 294, 18 },    // Cozy Draken House
                { 292, 28 },    // Cozy Exile Human House
                { 80, 11 },     // Cozy Granok House
                { 297, 26 },    // Spacious Aurin House
                { 298, 20 },    // Spacious Cassian House
                { 296, 23 },    // Spacious Chua House
                { 299, 21 },    // Spacious Draken House
                { 86, 17 },     // Spacious Exile Human House
                { 291, 27 },    // Spacious Granok House
                { 530, 32 },    // Underground Bunker
                { 534, 34 },    // Blackhole House
                { 543, 35 },    // Osun House
                { 18, 1 },      // Worksite? (No remodeling options)
                { 367, 25 },    // Spaceship ([Jumbo] Cockpit, [Jumbo] Wings)
                { 554, 37 },    // Aviary/Bird House (Feathered Falkrin, Mossy Hoogle) Birdhouse
                { 557, 27 },    // Royal Piglet (Entryway Large/Medium/Small, Peaked/Western Roof)
                { 37, 24 },     // Simple worksite? (No remodeling options)
                { 38, 1 },     // Simple worksite, again. (No remodeling options)
                { 19, 1 },     // Simple rocks and trees
                { 79, 1 }      // Nothing
            };// 38 has no remodel menu at all, 24 and 30 offer no remodel options.

            return residenceLookup.TryGetValue(plugItemId, out uint residenceId) ? residenceId : 0u;
        }

        public void RemoveHouse()
        {
            ResidenceInfoEntry = null;
            Wallpaper = 0;
            Roof = 0;
            Door = 0;
            Entryway = 0;

            saveMask |= ResidenceSaveMask.ResidenceInfo;
        }

        /// <summary>
        /// Return <see cref="Plot"/> at the supplied index.
        /// </summary>
        public Plot GetPlot(byte plotIndex)
        {
            return plots.FirstOrDefault(i => i.Index == plotIndex);
        }

        /// <summary>
        /// Return <see cref="Plot"/> that matches the supploed Plot Info ID.
        /// /// </summary>
        public Plot GetPlot(uint plotInfoId)
        {
            return plots.FirstOrDefault(i => i.PlotEntry.Id == plotInfoId);
        }
    }
}
