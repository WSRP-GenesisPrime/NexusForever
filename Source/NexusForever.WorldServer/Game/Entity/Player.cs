using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Auth;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.Database;
using NexusForever.Shared.Game;
using NexusForever.Shared.Game.Events;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.GameTable.Static;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.Achievement;
using NexusForever.WorldServer.Game.CharacterCache;
using NexusForever.WorldServer.Game.Contact;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Guild;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Game.Reputation;
using NexusForever.WorldServer.Game.Reputation.Static;
using NexusForever.WorldServer.Game.Setting;
using NexusForever.WorldServer.Game.Setting.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Game.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;

namespace NexusForever.WorldServer.Game.Entity
{
    public class Player : UnitEntity, ISaveAuth, ISaveCharacter, ICharacter
    {
        private readonly static ILogger log = LogManager.GetCurrentClassLogger();

        // TODO: move this to the config file
        private const double SaveDuration = 60d;

        public ulong CharacterId { get; }
        public string Name { get; }
        public Sex Sex { get; set; }
        public Race Race { get; set; }
        public Class Class { get; }
        public Faction Faction { get; }
        public List<float> Bones { get; } = new();
        public bool IsAdult {
            get
            {
                return Session.AccountRbacManager.HasPermission(RBAC.Static.Permission.Adult);
            }
        }

        private bool firstLoad = true;

        public CharacterFlag Flags
        {
            get => flags;
            set
            {
                flags = value;
                saveMask |= PlayerSaveMask.Flags;
            }
        }
        private CharacterFlag flags;

        public Path Path
        {
            get => path;
            set
            {
                path = value;
                PathActivatedTime = DateTime.UtcNow;
                saveMask |= PlayerSaveMask.Path;
            }
        }
        private Path path;

        public DateTime PathActivatedTime { get; private set; }

        public sbyte CostumeIndex
        {
            get => costumeIndex;
            set
            {
                costumeIndex = value;
                saveMask |= PlayerSaveMask.Costume;
            }
        }
        private sbyte costumeIndex;

        public InputSets InputKeySet
        {
            get => inputKeySet;
            set
            {
                inputKeySet = value;
                saveMask |= PlayerSaveMask.InputKeySet;
            }
        }
        private InputSets inputKeySet;

        public byte InnateIndex
        {
            get => innateIndex;
            set
            {
                innateIndex = value;
                saveMask |= PlayerSaveMask.Innate;
            }
        }
        private byte innateIndex;

        public DateTime CreateTime { get; }
        public double TimePlayedTotal { get; private set; }
        public double TimePlayedLevel { get; private set; }
        public double TimePlayedSession { get; private set; }

        /// <summary>
        /// Guid of the <see cref="WorldEntity"/> that currently being controlled by the <see cref="Player"/>.
        /// </summary>
        public uint ControlGuid { get; private set; }

        /// <summary>
        /// Guid of the <see cref="Vehicle"/> the <see cref="Player"/> is a passenger on.
        /// </summary>
        public uint VehicleGuid
        {
            get => MovementManager.GetPlatform() ?? 0u;
            set => MovementManager.SetPlatform(value);
        }

        /// <summary>
        /// Guid of the <see cref="VanityPet"/> currently summoned by the <see cref="Player"/>.
        /// </summary>
        public uint? VanityPetGuid { get; set; }

        public bool IsSitting => currentChairGuid != null;
        private uint? currentChairGuid;

        /// <summary>
        /// Whether or not this <see cref="Player"/> is currently in a state after using an emote. Setting this to false will let all nearby entities know that the state has been reset.
        /// </summary>
        public bool IsEmoting 
        {
            get => isEmoting;
            set
            {
                if (isEmoting && value == false)
                {
                    isEmoting = false;
                    EnqueueToVisible(new ServerEntityEmote
                    {
                        EmotesId = 0,
                        SourceUnitId = Guid
                    });
                    return;
                }

                isEmoting = value;
            }
        }
        private bool isEmoting;

        /// <summary>
        /// Returns if <see cref="Player"/> has premium signature subscription.
        /// </summary>
        public bool SignatureEnabled => Session.AccountRbacManager.HasPermission(Permission.Signature);

        public WorldSession Session { get; }

        /// <summary>
        /// Returns if <see cref="Player"/>'s client is currently in a loading screen.
        /// </summary>
        public bool IsLoading { get; set; } = true;

        /// <summary>
        /// Returns a <see cref="float"/> representing decimal value, in days, since the character was last online. Used by <see cref="ICharacter"/>.
        /// </summary>
        /// <remarks>
        /// 0 is always returned for online players.
        /// </remarks>
        public float? GetOnlineStatus() => 0f;

        private Dictionary</*label*/uint, Customisation> characterCustomisations = new Dictionary<uint, Customisation>();
        private HashSet<Customisation> deletedCharacterCustomisations = new HashSet<Customisation>();
        private Dictionary<ItemSlot, Appearance> characterAppearances = new Dictionary<ItemSlot, Appearance>();
        private HashSet<Appearance> deletedCharacterAppearances = new HashSet<Appearance>();
        private List<Bone> characterBones = new List<Bone>();
        private HashSet<Bone> deletedCharacterBones = new HashSet<Bone>();
        public Inventory Inventory { get; }
        public CurrencyManager CurrencyManager { get; }
        public PathManager PathManager { get; }
        public TitleManager TitleManager { get; }
        public SpellManager SpellManager { get; }
        public CostumeManager CostumeManager { get; }
        public PetCustomisationManager PetCustomisationManager { get; }
        public KeybindingManager KeybindingManager { get; }
        public DatacubeManager DatacubeManager { get; }
        public MailManager MailManager { get; }
        public ZoneMapManager ZoneMapManager { get; }
        public QuestManager QuestManager { get; }
        public CharacterAchievementManager AchievementManager { get; }
        public SupplySatchelManager SupplySatchelManager { get; }
        public XpManager XpManager { get; }
        public ReputationManager ReputationManager { get; }
        public GuildManager GuildManager { get; }
        public ChatManager ChatManager { get; }
        public ResidenceManager ResidenceManager { get; }
        public ContactManager ContactManager { get; }

        public VendorInfo SelectedVendorInfo { get; set; } // TODO unset this when too far away from vendor

        private UpdateTimer saveTimer = new(SaveDuration);
        private PlayerSaveMask saveMask;

        private LogoutManager logoutManager;

        /// <summary>
        /// Returns if <see cref="Player"/> can teleport.
        /// </summary>
        public bool CanTeleport() => pendingTeleport == null;
        private PendingTeleport pendingTeleport;
        
        public uint HousePreviousWorld { get; set; }
        public Vector3 HousePreviousLocation { get; set; }
        public Vector3 HouseOutsideLocation {
            get => houseOutsideLocation;
            set
            {
                houseOutsideLocation = value;
                housingMapTeleport.Reset(true);
            }
        }
        private Vector3 houseOutsideLocation = Vector3.Zero;
        private UpdateTimer housingMapTeleport = new UpdateTimer(1d);
        public bool CanUseHousingDoors() => housingMapTeleport.HasElapsed;

        /// <summary>
        /// Create a new <see cref="Player"/> from supplied <see cref="WorldSession"/> and <see cref="CharacterModel"/>.
        /// </summary>
        public Player(WorldSession session, CharacterModel model)
            : base(EntityType.Player)
        {
            ActivationRange = BaseMap.DefaultVisionRange;

            Session         = session;

            CharacterId     = model.Id;
            Name            = model.Name;
            Sex             = (Sex)model.Sex;
            Race            = (Race)model.Race;
            Class           = (Class)model.Class;
            path            = (Path)model.ActivePath;
            PathActivatedTime = model.PathActivatedTimestamp;
            CostumeIndex    = model.ActiveCostumeIndex;
            InputKeySet     = (InputSets)model.InputKeySet;
            Faction         = (Faction)model.FactionId;
            Faction1        = (Faction)model.FactionId;
            Faction2        = (Faction)model.FactionId;
            innateIndex     = model.InnateIndex;
            flags           = (CharacterFlag)model.Flags;

            CreateTime      = model.CreateTime;
            TimePlayedTotal = model.TimePlayedTotal;
            TimePlayedLevel = model.TimePlayedLevel;

            Session.EntitlementManager.Initialise(model);

            foreach (CharacterStatModel statModel in model.Stat)
                stats.Add((Stat)statModel.Stat, new StatValue(statModel));

            // managers
            CostumeManager          = new CostumeManager(this, session.Account, model);
            Inventory               = new Inventory(this, model);
            CurrencyManager         = new CurrencyManager(this, model);
            PathManager             = new PathManager(this, model);
            TitleManager            = new TitleManager(this, model);
            SpellManager            = new SpellManager(this, model);
            PetCustomisationManager = new PetCustomisationManager(this, model);
            KeybindingManager       = new KeybindingManager(this, session.Account, model);
            DatacubeManager         = new DatacubeManager(this, model);
            MailManager             = new MailManager(this, model);
            ZoneMapManager          = new ZoneMapManager(this, model);
            QuestManager            = new QuestManager(this, model);
            AchievementManager      = new CharacterAchievementManager(this, model);
            SupplySatchelManager    = new SupplySatchelManager(this, model);
            XpManager               = new XpManager(this, model);
            ReputationManager       = new ReputationManager(this, model);
            GuildManager            = new GuildManager(this, model);
            ContactManager          = new ContactManager(this, model);
            ChatManager             = new ChatManager(this);
            ResidenceManager        = new ResidenceManager(this);

            Costume costume = null;
            if (CostumeIndex >= 0)
                costume = CostumeManager.GetCostume((byte)CostumeIndex);

            SetAppearance(Inventory.GetItemVisuals(costume));
            SetAppearance(model.Appearance
                .Select(a => new ItemVisual
                {
                    Slot      = (ItemSlot)a.Slot,
                    DisplayId = a.DisplayId
                }));

            // Store Character Customisation models in memory so if changes occur, they can be removed.
            foreach (CharacterAppearanceModel characterAppearance in model.Appearance)
                characterAppearances.Add((ItemSlot)characterAppearance.Slot, new Appearance(characterAppearance));

            foreach (CharacterCustomisationModel characterCustomisation in model.Customisation)
                characterCustomisations.Add(characterCustomisation.Label, new Customisation(characterCustomisation));

            foreach (CharacterBoneModel bone in model.Bone.OrderBy(bone => bone.BoneIndex))
            {
                Bones.Add(bone.Bone);
                characterBones.Add(new Bone(bone));
            }

            BuildBaseProperties();

            SetStat(Stat.Sheathed, 1u);

            // temp
            SetStat(Stat.Dash, 200F);
            // sprint
            SetStat(Stat.Resource0, 500f);
            SetStat(Stat.Shield, 450u);

            SetStat(Stat.Health, (uint) GetPropertyValue(Property.BaseHealth));

            CharacterManager.Instance.RegisterPlayer(this);
        }

        public override void BuildBaseProperties()
        {
            var baseProperties = AssetManager.Instance.GetCharacterBaseProperties();
            foreach (PropertyValue propertyValue in baseProperties)
            {
                float value = propertyValue.Value; // Intentionally copying value so that the PropertyValue does not get modified inside AssetManager

                if (propertyValue.Property == Property.BaseHealth || propertyValue.Property == Property.AssaultRating || propertyValue.Property == Property.SupportRating)
                    value *= Level;

                SetBaseProperty(propertyValue.Property, value);
            }

            var classProperties = AssetManager.Instance.GetCharacterClassBaseProperties(Class);
            foreach (PropertyValue propertyValue in classProperties)
            {
                float value = propertyValue.Value; // Intentionally copying value so that the PropertyValue does not get modified inside AssetManager

                SetBaseProperty(propertyValue.Property, value);
            }

            base.BuildBaseProperties();
        }

        public override void Update(double lastTick)
        {
            if (logoutManager != null)
            {
                // don't process world updates while logout is finalising
                if (logoutManager.ReadyToLogout)
                    return;

                logoutManager.Update(lastTick);
            }

            base.Update(lastTick);
            TitleManager.Update(lastTick);
            SpellManager.Update(lastTick);
            CostumeManager.Update(lastTick);
            QuestManager.Update(lastTick);

            if (housingMapTeleport.IsTicking)
                housingMapTeleport.Update(lastTick);

            saveTimer.Update(lastTick);
            if (saveTimer.HasElapsed)
            {
                double timeSinceLastSave = GetTimeSinceLastSave();
                TimePlayedSession += timeSinceLastSave;
                TimePlayedLevel += timeSinceLastSave;
                TimePlayedTotal += timeSinceLastSave;

                Save();
            }
        }

        /// <summary>
        /// Modifies the appearance customisation of this <see cref="Player"/>. Called directly by a packet handler.
        /// </summary>
        public void SetCharacterCustomisation(Dictionary<uint, uint> customisations, List<float> bones, Race newRace, Sex newSex, bool usingServiceTokens)
        {
            // Set Sex and Race
            Sex = newSex;
            Race = newRace; // TODO: Ensure new Race is on the same faction

            List<ItemSlot> itemSlotsModified = new List<ItemSlot>();
            // Build models for all new customisations and store in customisations caches. The client sends through everything needed on every change.
            foreach ((uint label, uint value) in customisations)
            {
                if (characterCustomisations.TryGetValue(label, out Customisation customisation))
                    customisation.Value = value;
                else
                    characterCustomisations.TryAdd(label, new Customisation(CharacterId, label, value));

                foreach (CharacterCustomizationEntry entry in AssetManager.Instance.GetCharacterCustomisation(customisations, (uint)newRace, (uint)newSex, label, value))
                {
                    if (characterAppearances.TryGetValue((ItemSlot)entry.ItemSlotId, out Appearance appearance))
                        appearance.DisplayId = (ushort)entry.ItemDisplayId;
                    else
                        characterAppearances.TryAdd((ItemSlot)entry.ItemSlotId, new Appearance(CharacterId, (ItemSlot)entry.ItemSlotId, (ushort)entry.ItemDisplayId));

                    // This is to track slots which are modified
                    itemSlotsModified.Add((ItemSlot)entry.ItemSlotId);
                }
            }

            for (int i = 0; i < bones.Count; i++)
            {
                if (i > characterBones.Count - 1)
                    characterBones.Add(new Bone(CharacterId, (byte)i, bones[i]));
                else
                {
                    var bone = characterBones.FirstOrDefault(x => x.BoneIndex == i);
                    if (bone != null)
                        bone.BoneValue = bones[i];
                }
            }

            // Cleanup the unused customisations
            foreach (ItemSlot slot in characterAppearances.Keys.Except(itemSlotsModified).ToList())
            {
                if (characterAppearances.TryGetValue(slot, out Appearance appearance))
                {
                    characterAppearances.Remove(slot);
                    appearance.Delete();
                    deletedCharacterAppearances.Add(appearance);
                }
            }
            foreach (uint key in characterCustomisations.Keys.Except(customisations.Keys).ToList())
            {
                if (characterCustomisations.TryGetValue(key, out Customisation customisation))
                {
                    characterCustomisations.Remove(key);
                    customisation.Delete();
                    deletedCharacterCustomisations.Add(customisation);
                }
            }
            if (Bones.Count > bones.Count)
            {
                for (int i = Bones.Count; i >= bones.Count; i--)
                {
                    Bone bone = characterBones[i];

                    if (bone != null)
                    {
                        characterBones.RemoveAt(i);
                        bone.Delete();
                        deletedCharacterBones.Add(bone);
                    }
                }
            }

            // Update Player appearance values
            SetAppearance(characterAppearances.Values
                .Select(a => new ItemVisual
                {
                    Slot = a.ItemSlot,
                    DisplayId = a.DisplayId
                }));

            Bones.Clear();
            foreach (Bone bone in characterBones.OrderBy(bone => bone.BoneIndex))
                Bones.Add(bone.BoneValue);

            // Update surrounding entities, including the player, with new appearance
            EmitVisualUpdate();

            // TODO: Charge the player for service

            // Enqueue the appearance changes to be saved to the DB.
            saveMask |= PlayerSaveMask.Appearance;
        }

        /// <summary>
        /// Update surrounding <see cref="WorldEntity"/>, including the <see cref="Player"/>, with a fresh appearance dataset.
        /// </summary>
        public void EmitVisualUpdate()
        {
            Costume costume = null;
            if (CostumeIndex >= 0)
                costume = CostumeManager.GetCostume((byte)CostumeIndex);

            var entityVisualUpdate = new ServerEntityVisualUpdate
            {
                UnitId = Guid,
                Sex = (byte)Sex,
                Race = (byte)Race
            };

            foreach (Appearance characterAppearance in characterAppearances.Values)
                entityVisualUpdate.ItemVisuals.Add(new ItemVisual
                {
                    Slot = characterAppearance.ItemSlot,
                    DisplayId = characterAppearance.DisplayId
                });

            foreach (var itemVisual in Inventory.GetItemVisuals(costume))
                entityVisualUpdate.ItemVisuals.Add(itemVisual);

            if(costume != null)
            {
                foreach (var overrideItem in costume.getOverrides())
                {
                    if(overrideItem.Value == null)
                    {
                        continue;
                    }
                    ItemVisual iv = entityVisualUpdate.ItemVisuals.Where(iv => iv.Slot == overrideItem.Key).FirstOrDefault();
                    if(iv != null)
                    {
                        iv.DisplayId = (ushort) overrideItem.Value;
                    }
                    else
                    {
                        iv = new ItemVisual
                        {
                            DisplayId = (ushort)overrideItem.Value,
                            Slot = overrideItem.Key,
                            DyeData = 0
                        };
                        entityVisualUpdate.ItemVisuals.Add(iv);
                    }
                }
            }

            EnqueueToVisible(entityVisualUpdate, true);

            EnqueueToVisible(new ServerEntityBoneUpdate
            {
                UnitId = Guid,
                Bones = Bones.ToList()
            }, true);
        }

        /// <summary>
        /// Save <see cref="Player"/> to database, invoke supplied <see cref="Action"/> once save is complete.
        /// </summary>
        /// <remarks>
        /// This is a delayed save, <see cref="AuthContext"/> changes are saved first followed by <see cref="CharacterContext"/> changes.
        /// Packets for session will not be handled until save is complete.
        /// </remarks>
        public void Save(Action callback = null)
        {
            Session.Events.EnqueueEvent(new TaskEvent(DatabaseManager.Instance.AuthDatabase.Save(Save),
            () =>
            {
                Session.Events.EnqueueEvent(new TaskEvent(DatabaseManager.Instance.CharacterDatabase.Save(Save),
                () =>
                {
                    callback?.Invoke();
                    Session.CanProcessPackets = true;
                    saveTimer.Resume();
                }));
            }));

            saveTimer.Reset(false);

            // prevent packets from being processed until asynchronous player save task is complete
            Session.CanProcessPackets = false;
        }

        /// <summary>
        /// Save <see cref="Player"/> to database.
        /// </summary>
        /// <remarks>
        /// This is an instant save, <see cref="AuthContext"/> changes are saved first followed by <see cref="CharacterContext"/> changes.
        /// This will block the calling thread until the database save is complete. 
        /// </remarks>
        public async void SaveDirect()
        {
            await DatabaseManager.Instance.AuthDatabase.Save(Save);
            await DatabaseManager.Instance.CharacterDatabase.Save(Save);
        }

        /// <summary>
        /// Save database changes for <see cref="Player"/> to <see cref="AuthContext"/>.
        /// </summary>
        public void Save(AuthContext context)
        {
            Session.AccountRbacManager.Save(context);
            Session.GenericUnlockManager.Save(context);
            Session.AccountCurrencyManager.Save(context);
            Session.EntitlementManager.Save(context);

            CostumeManager.Save(context);
            KeybindingManager.Save(context);
        }

        /// <summary>
        /// Save database changes for <see cref="Player"/> to <see cref="CharacterContext"/>.
        /// </summary>
        public void Save(CharacterContext context)
        {
            var model = new CharacterModel
            {
                Id = CharacterId
            };

            EntityEntry<CharacterModel> entity = context.Attach(model);

            if (saveMask != PlayerSaveMask.None)
            {
                if ((saveMask & PlayerSaveMask.Location) != 0)
                {
                    model.LocationX = Position.X;
                    entity.Property(p => p.LocationX).IsModified = true;

                    model.LocationY = Position.Y;
                    entity.Property(p => p.LocationY).IsModified = true;

                    model.LocationZ = Position.Z;
                    entity.Property(p => p.LocationZ).IsModified = true;

                    model.RotationX = Rotation.X;
                    entity.Property(p => p.RotationX).IsModified = true;

                    model.RotationY = Rotation.Y;
                    entity.Property(p => p.RotationY).IsModified = true;

                    model.RotationZ = Rotation.Z;
                    entity.Property(p => p.RotationZ).IsModified = true;

                    model.WorldId = (ushort)Map.Entry.Id;
                    entity.Property(p => p.WorldId).IsModified = true;

                    model.WorldZoneId = (ushort)Zone.Id;
                    entity.Property(p => p.WorldZoneId).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Path) != 0)
                {
                    model.ActivePath = (uint)Path;
                    entity.Property(p => p.ActivePath).IsModified = true;
                    model.PathActivatedTimestamp = PathActivatedTime;
                    entity.Property(p => p.PathActivatedTimestamp).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Costume) != 0)
                {
                    model.ActiveCostumeIndex = CostumeIndex;
                    entity.Property(p => p.ActiveCostumeIndex).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.InputKeySet) != 0)
                {
                    model.InputKeySet = (sbyte)InputKeySet;
                    entity.Property(p => p.InputKeySet).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Flags) != 0)
                {
                    model.Flags = (uint)Flags;
                    entity.Property(p => p.Flags).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Innate) != 0)
                {
                    model.InnateIndex = InnateIndex;
                    entity.Property(p => p.InnateIndex).IsModified = true;
                }

                if ((saveMask & PlayerSaveMask.Appearance) != 0)
                {
                    model.Race = (byte)Race;
                    entity.Property(p => p.Race).IsModified = true;

                    model.Sex = (byte)Sex;
                    entity.Property(p => p.Sex).IsModified = true;

                    foreach (Appearance characterAppearance in deletedCharacterAppearances)
                        characterAppearance.Save(context);
                    foreach (Bone characterBone in deletedCharacterBones)
                        characterBone.Save(context);
                    foreach (Customisation characterCustomisation in deletedCharacterCustomisations)
                        characterCustomisation.Save(context);

                    deletedCharacterAppearances.Clear();
                    deletedCharacterBones.Clear();
                    deletedCharacterCustomisations.Clear();

                    foreach (Appearance characterAppearance in characterAppearances.Values)
                        characterAppearance.Save(context);
                    foreach (Bone characterBone in characterBones)
                        characterBone.Save(context);
                    foreach (Customisation characterCustomisation in characterCustomisations.Values)
                        characterCustomisation.Save(context);
                }

                saveMask = PlayerSaveMask.None;
            }

            model.TimePlayedLevel = (uint)TimePlayedLevel;
            entity.Property(p => p.TimePlayedLevel).IsModified = true;
            model.TimePlayedTotal = (uint)TimePlayedTotal;
            entity.Property(p => p.TimePlayedTotal).IsModified = true;
            model.LastOnline = DateTime.UtcNow;
            entity.Property(p => p.LastOnline).IsModified = true;

            foreach (StatValue stat in stats.Values)
                stat.SaveCharacter(CharacterId, context);

            Inventory.Save(context);
            CurrencyManager.Save(context);
            PathManager.Save(context);
            TitleManager.Save(context);
            CostumeManager.Save(context);
            PetCustomisationManager.Save(context);
            KeybindingManager.Save(context);
            SpellManager.Save(context);
            DatacubeManager.Save(context);
            MailManager.Save(context);
            ZoneMapManager.Save(context);
            QuestManager.Save(context);
            AchievementManager.Save(context);
            SupplySatchelManager.Save(context);
            XpManager.Save(context);
            ReputationManager.Save(context);
            GuildManager.Save(context);
            ContactManager.Save(context);

            Session.EntitlementManager.Save(context);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new PlayerEntityModel
            {
                Id        = CharacterId,
                RealmId   = WorldServer.RealmId,
                Name      = Name,
                Race      = Race,
                Class     = Class,
                Sex       = Sex,
                Bones     = Bones,
                Title     = TitleManager.ActiveTitleId,
                GuildIds  = GuildManager
                    .Select(g => g.Id)
                    .ToList(),
                GuildName = GuildManager.GuildAffiliation?.Name,
                GuildType = GuildManager.GuildAffiliation?.Type ?? GuildType.None,
                PvPFlag   = PvPFlag.Disabled
            };
        }

        public override void CancelEffect(uint castingId)
        {
            base.CancelEffect(castingId);
            if (VehicleGuid != 0u)
            {
                Mount mount = GetVisible<Mount>(VehicleGuid);
                if (mount != null && mount.castingId == castingId)
                {
                    Dismount();
                }
            }
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            if(guid <= 0)
            {
                log.Warn($"Adding player {Name} ({CharacterId}) to map {map.Entry.Id} with guid 0!");
            }
            else
            {
                log.Info($"Adding player {Name} ({CharacterId}) to map {map.Entry.Id} with guid {guid}.");
            }
            IsLoading = true;

            Session.EnqueueMessageEncrypted(new ServerChangeWorld
            {
                WorldId  = (ushort)map.Entry.Id,
                Position = new Position(vector),
                Yaw      = Rotation.X
            });

            // this must come before OnAddToMap
            // the client UI initialises the Holomark checkboxes during OnDocumentReady
            SendCharacterFlagsUpdated();

            base.OnAddToMap(map, guid, vector);

            // resummon vanity pet if it existed before teleport
            if (pendingTeleport?.VanityPetId != null)
            {
                var vanityPet = new VanityPet(this, pendingTeleport.VanityPetId.Value);

                var position = new MapPosition
                {
                    Position = Position
                };

                if (map.CanEnter(vanityPet, position))
                    map.EnqueueAdd(vanityPet, position);
            }

            pendingTeleport = null;

            SendPacketsAfterAddToMap();
            if (PreviousMap == null)
                OnLogin();
        }

        public override void OnRelocate(Vector3 vector)
        {
            base.OnRelocate(vector);
            saveMask |= PlayerSaveMask.Location;

            ZoneMapManager.OnRelocate(vector);
        }

        protected override void OnZoneUpdate()
        {
            if (Zone != null)
            {
                TextTable tt = GameTableManager.Instance.GetTextTable(Language.English);
                if (tt != null)
                {
                    //GlobalChatManager.Instance.SendMessage(Session, $"New Zone: ({Zone.Id}){tt.GetEntry(Zone.LocalizedTextIdName)}");
                }

                uint tutorialId = AssetManager.Instance.GetTutorialIdForZone(Zone.Id);
                if (tutorialId > 0)
                {
                    Session.EnqueueMessageEncrypted(new ServerTutorial
                    {
                        TutorialId = tutorialId
                    });
                }

                QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterZone, Zone.Id, 1);
            }

            ZoneMapManager.OnZoneUpdate();
        }

        private void SendPacketsAfterAddToMap()
        {
            DateTime start = DateTime.UtcNow;

            SendInGameTime();
            PathManager.SendInitialPackets();
            BuybackManager.Instance.SendBuybackItems(this);

            ResidenceManager.SendHousingBasics();
            ContactManager.OnLogin();

            Session.EnqueueMessageEncrypted(new ServerHousingNeighbors());
            Session.EnqueueMessageEncrypted(new ServerInstanceSettings());

            SetControl(this);

            CostumeManager.SendInitialPackets();

            if (firstLoad)
            {
                var playerCreate = new ServerPlayerCreate
                {
                    ItemProficiencies = GetItemProficiencies(),
                    FactionData = new ServerPlayerCreate.Faction
                    {
                        FactionId = Faction1, // This does not do anything for the player's "main" faction. Exiles/Dominion
                        FactionReputations = ReputationManager
                            .Select(r => new ServerPlayerCreate.Faction.FactionReputation
                            {
                                FactionId = r.Id,
                                Value = r.Amount
                            })
                            .ToList()
                    },
                    ActiveCostumeIndex = CostumeIndex,
                    InputKeySet = (uint)InputKeySet,
                    CharacterEntitlements = Session.EntitlementManager.GetCharacterEntitlements()
                        .Select(e => new ServerPlayerCreate.CharacterEntitlement
                        {
                            Entitlement = e.Type,
                            Count = e.Amount
                        })
                        .ToList(),
                    TradeskillMaterials = SupplySatchelManager.BuildNetworkPacket(),
                    Xp = XpManager.TotalXp,
                    RestBonusXp = XpManager.RestBonusXp
                };

                foreach (Currency currency in CurrencyManager)
                    playerCreate.Money[(byte)currency.Id - 1] = currency.Amount;

                foreach (Item item in Inventory
                    .Where(b => b.Location != InventoryLocation.Ability)
                    .SelectMany(i => i))
                {
                    playerCreate.Inventory.Add(new InventoryItem
                    {
                        Item = item.BuildNetworkItem(),
                        Reason = ItemUpdateReason.NoReason
                    });
                }

                playerCreate.SpecIndex = SpellManager.ActiveActionSet;
                Session.EnqueueMessageEncrypted(playerCreate);

                firstLoad = false;
            }

            TitleManager.SendTitles();
            SpellManager.SendInitialPackets();
            PetCustomisationManager.SendInitialPackets();
            KeybindingManager.SendInitialPackets();
            DatacubeManager.SendInitialPackets();
            MailManager.SendInitialPackets();
            ZoneMapManager.SendInitialPackets();
            Session.AccountCurrencyManager.SendInitialPackets();
            QuestManager.SendInitialPackets();
            AchievementManager.SendInitialPackets(null);
            Session.EntitlementManager.SendInitialPackets();

            Session.EnqueueMessageEncrypted(new ServerPlayerInnate
            {
                InnateIndex = InnateIndex
            });

            log.Trace($"Player {Name} took {(DateTime.UtcNow - start).TotalMilliseconds}ms to send packets after add to map.");
        }

        public ItemProficiency GetItemProficiencies()
        {
            //TODO: Store proficiencies in DB table and load from there. Do they change ever after creation? Perhaps something for use on custom servers?
            ClassEntry classEntry = GameTableManager.Instance.Class.GetEntry((ulong)Class);
            return (ItemProficiency)classEntry.StartingItemProficiencies;
        }

        public override void OnRemoveFromMap()
        {
            if (Guid <= 0)
            {
                log.Warn($"Removing player {Name} ({CharacterId}) from map {Map.Entry.Id} with guid 0!");
            }
            else
            {
                log.Info($"Removing player {Name} ({CharacterId}) from map {Map.Entry.Id} with guid {Guid}.");
            }
            DestroyDependents();
            base.OnRemoveFromMap();
        }

        public override void AddVisible(GridEntity entity)
        {
            base.AddVisible(entity);
            Session.EnqueueMessageEncrypted(((WorldEntity)entity).BuildCreatePacket());

            if (entity is Player player)
                player.PathManager.SendSetUnitPathTypePacket();

            if (entity == this)
            {
                Session.EnqueueMessageEncrypted(new ServerPlayerChanged
                {
                    Guid     = entity.Guid,
                    Unknown1 = 1
                });
            }
        }

        public override void RemoveVisible(GridEntity entity)
        {
            base.RemoveVisible(entity);

            if (entity != this)
            {
                Session.EnqueueMessageEncrypted(new ServerEntityDestroy
                {
                    Guid     = entity.Guid,
                    Unknown0 = true
                });
            }
        }

        /// <summary>
        /// Set the <see cref="WorldEntity"/> that currently being controlled by the <see cref="Player"/>.
        /// </summary>
        public void SetControl(WorldEntity entity)
        {
            ControlGuid = entity.Guid;
            entity.ControllerGuid = Guid;

            Session.EnqueueMessageEncrypted(new ServerMovementControl
            {
                Ticket    = 1,
                Immediate = true,
                UnitId    = entity.Guid
            });
        }

        /// <summary>
        /// Start delayed logout with optional supplied time and <see cref="LogoutReason"/>.
        /// </summary>
        public void LogoutStart(double timeToLogout = 30d, LogoutReason reason = LogoutReason.None, bool requested = true)
        {
            if (logoutManager != null)
                return;

            logoutManager = new LogoutManager(timeToLogout, reason, requested);

            Session.EnqueueMessageEncrypted(new ServerLogoutUpdate
            {
                TimeTillLogout     = (uint)timeToLogout * 1000,
                Unknown0           = false,
                SignatureBonusData = new ServerLogoutUpdate.SignatureBonuses
                {
                    // see FillSignatureBonuses in ExitWindow.lua for more information
                    Xp                = 0,
                    ElderPoints       = 0,
                    Currencies        = new ulong[15],
                    AccountCurrencies = new ulong[19]
                }
            });
        }

        /// <summary>
        /// Cancel the current logout, this will fail if the timer has already elapsed.
        /// </summary>
        public void LogoutCancel()
        {
            // can't cancel logout if timer has already elapsed
            if (logoutManager?.ReadyToLogout ?? false)
                return;

            logoutManager = null;
        }

        /// <summary>
        /// Finishes the current logout, saving and cleaning up the <see cref="Player"/> before redirect to the character screen.
        /// </summary>
        public void LogoutFinish()
        {
            if (logoutManager == null)
                throw new InvalidPacketValueException();

            Session.EnqueueMessageEncrypted(new ServerLogout
            {
                Requested = logoutManager.Requested,
                Reason    = logoutManager.Reason
            });

            CleanUp();
        }

        /// <summary>
        /// Save to the database, remove from the world and release from parent <see cref="WorldSession"/>.
        /// </summary>
        public void CleanUp()
        {
            ContactManager.OnLogout();
            CharacterManager.Instance.DeregisterPlayer(this);
            PlayerCleanupManager.Track(Session.Account);

            log.Trace($"Attempting to start cleanup for character {Name}({CharacterId})...");

            Session.Events.EnqueueEvent(new TimeoutPredicateEvent(TimeSpan.FromSeconds(15), CanCleanup,
                () =>
            {
                try
                {
                    log.Trace($"Cleanup for character {Name}({CharacterId}) has started...");

                    OnLogout();

                    Save(() =>
                    {
                        if (Map != null)
                            RemoveFromMap();

                        Session.Player = null;
                    });
                }
                finally
                {
                    PlayerCleanupManager.Untrack(Session.Account);
                    log.Trace($"Cleanup for character {Name}({CharacterId}) has completed.");
                }
            }));
        }

        private bool CanCleanup()
        {
            return pendingTeleport == null;
        }

        private void OnLogin()
        {
            string motd = WorldServer.RealmMotd;
            if (motd?.Length > 0)
                GlobalChatManager.Instance.SendMessage(Session, motd, "MOTD", ChatChannelType.Realm);

            GuildManager.OnLogin();
            ChatManager.OnLogin();
            GlobalChatManager.Instance.JoinDefaultChatChannels(this);

            ShutdownManager.Instance.OnLogin(this);
        }

        private void OnLogout()
        {
            GuildManager.OnLogout();
            ChatManager.OnLogout();
            GlobalChatManager.Instance.LeaveDefaultChatChannels(this);
        }

        /// <summary>
        /// Teleport <see cref="Player"/> to location defined in supplied <see cref="WorldLocation2Entry"/>.
        /// </summary>
        public void TeleportTo(WorldLocation2Entry worldLocation2Entry, ulong? instanceId = null)
        {
            if (worldLocation2Entry == null)
                throw new ArgumentException(nameof(worldLocation2Entry));

            WorldEntry entry = GameTableManager.Instance.World.GetEntry(worldLocation2Entry.WorldId);
            if (entry == null)
                throw new ArgumentException();

            Rotation = new Quaternion(worldLocation2Entry.Facing0, worldLocation2Entry.Facing1, worldLocation2Entry.Facing2, worldLocation2Entry.Facing3).ToEulerDegrees() * (float)Math.PI * 2 / 360;
            TeleportTo((ushort) entry.Id, worldLocation2Entry.Position0, worldLocation2Entry.Position1, worldLocation2Entry.Position2, instanceId);
        }

        /// <summary>
        /// Teleport <see cref="Player"/> to supplied location.
        /// </summary>
        public void TeleportTo(ushort worldId, float x, float y, float z, ulong? instanceId = null, TeleportReason reason = TeleportReason.Relocate)
        {
            WorldEntry entry = GameTableManager.Instance.World.GetEntry(worldId);
            if (entry == null)
                throw new ArgumentException($"{worldId} is not a valid world id!");

            TeleportTo(entry, new Vector3(x, y, z), instanceId, reason);
        }

        /// <summary>
        /// Teleport <see cref="Player"/> to supplied location.
        /// </summary>
        public void TeleportTo(WorldEntry entry, Vector3 position, ulong? instanceId = null, TeleportReason reason = TeleportReason.Relocate)
        {
            TeleportTo(new MapPosition
            {
                Info     = new MapInfo
                {
                    Entry      = entry,
                    InstanceId = instanceId
                },
                Position = position
            }, reason);
        }

        /// <summary>
        /// Teleport <see cref="Player"/> to supplied location.
        /// </summary>
        public void TeleportTo(MapPosition mapPosition, TeleportReason reason = TeleportReason.Relocate)
        {
            if (!CanTeleport())
            {
                SendGenericError(GenericError.InstanceTransferPending);
                return;
            }

            if (DisableManager.Instance.IsDisabled(DisableType.World, mapPosition.Info.Entry.Id))
            {
                SendSystemMessage($"Unable to teleport to world {mapPosition.Info.Entry.Id} because it is disabled.");
                return;
            }

            // store vanity pet summoned before teleport so it can be summoned again after being added to the new map
            uint? vanityPetId = null;
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                vanityPetId = pet?.Creature.Id;
            }

            pendingTeleport = new PendingTeleport
            {
                Reason      = reason,
                MapPosition = mapPosition,
                VanityPetId = vanityPetId
            };

            MapManager.Instance.AddToMap(this, mapPosition);
            log.Trace($"Teleporting {Name}({CharacterId}) to map: {mapPosition.Info.Entry.Id}, instance: {mapPosition.Info.InstanceId ?? 0ul}.");
        }

        /// <summary>
        /// Invoked when <see cref="Player"/> teleport fails.
        /// </summary>
        public void OnTeleportToFailed(GenericError error)
        {
            if (Map != null)
            {
                SendGenericError(error);
                pendingTeleport = null;

                log.Trace($"Error {error} occured during teleport for {Name}({CharacterId})!");
            }
            else
            {
                // player failed prerequisites to enter map on login
                // can not proceed, disconnect the client with a message
                Session.EnqueueMessageEncrypted(new ServerForceKick
                {
                    Reason = ForceKickReason.WorldDisconnect
                });

                log.Trace($"Error {error} occured during teleport for {Name}({CharacterId}), client will be disconnected!");
            }
        }

        /// <summary>
        /// Used to send the current in game time to this player
        /// </summary>
        private void SendInGameTime()
        {
            uint lengthOfInGameDayInSeconds = ConfigurationManager<WorldServerConfiguration>.Instance.Config.LengthOfInGameDay;
            if (lengthOfInGameDayInSeconds == 0u)
                lengthOfInGameDayInSeconds = (uint)TimeSpan.FromHours(3.5d).TotalSeconds; // Live servers were 3.5h per in game day

            double timeOfDay = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds / lengthOfInGameDayInSeconds % 1;

            Session.EnqueueMessageEncrypted(new ServerTimeOfDay
            {
                TimeOfDay = (uint)(timeOfDay * TimeSpan.FromDays(1).TotalSeconds),
                LengthOfDay = lengthOfInGameDayInSeconds
            });
        }

        /// <summary>
        /// Reset and restore default appearance for <see cref="Player"/>.
        /// </summary>
        public void ResetAppearance()
        {
            DisplayInfo = 0;

            EnqueueToVisible(new ServerEntityVisualUpdate
            {
                UnitId      = Guid,
                Race        = (byte)Race,
                Sex         = (byte)Sex,
                ItemVisuals = GetAppearance().ToList()
            }, true);
        }

        /// <summary>
        /// Make <see cref="Player"/> sit on provided <see cref="WorldEntity"/>.
        /// </summary>
        public void Sit(WorldEntity chair)
        {
            if (IsSitting)
                Unsit();

            currentChairGuid = chair.Guid;

            // TODO: Emit interactive state from the entity instance itself
            chair.EnqueueToVisible(new ServerEntityInteractiveUpdate
            {
                UnitId = chair.Guid,
                InUse  = true
            }, true);
            EnqueueToVisible(new ServerUnitSetChair
            {
                UnitId      = Guid,
                UnitIdChair = chair.Guid,
                WaitForUnit = false
            }, true);
        }

        /// <summary>
        /// Remove <see cref="Player"/> from the <see cref="WorldEntity"/> it is sitting on.
        /// </summary>
        public void Unsit()
        {
            if (!IsSitting)
                return;

            WorldEntity currentChair = GetVisible<WorldEntity>(currentChairGuid.Value);
            if (currentChair == null)
                throw new InvalidOperationException();

            // TODO: Emit interactive state from the entity instance itself
            currentChair.EnqueueToVisible(new ServerEntityInteractiveUpdate
            {
                UnitId = currentChair.Guid,
                InUse  = false
            }, true);
            EnqueueToVisible(new ServerUnitSetChair
            {
                UnitId      = Guid,
                UnitIdChair = 0,
                WaitForUnit = false
            }, true);

            currentChairGuid = null;
        }

        /// <summary>
        /// Shortcut method to grant XP to the player
        /// </summary>
        public void GrantXp(uint xp, ExpReason reason = ExpReason.Cheat)
        {
            XpManager.GrantXp(xp, reason);
        }

        /// <summary>
        /// Send <see cref="GenericError"/> to <see cref="Player"/>.
        /// </summary>
        public void SendGenericError(GenericError error)
        {
            Session.EnqueueMessageEncrypted(new ServerGenericError
            {
                Error = error
            });
        }

        /// <summary>
        /// Send message to <see cref="Player"/> using the <see cref="ChatChannel.System"/> channel.
        /// </summary>
        /// <param name="text"></param>
        public void SendSystemMessage(string text)
        {
            Session.EnqueueMessageEncrypted(new ServerChat
            {
                Channel = new Channel
                {
                    Type = ChatChannelType.System
                },
                Text    = text
            });
        }

        /// <summary>
        /// Returns whether this <see cref="Player"/> is allowed to summon or be added to a mount
        /// </summary>
        public bool CanMount()
        {
            return VehicleGuid == 0u && pendingTeleport == null && logoutManager == null;
        }

        /// <summary>
        /// Dismounts this <see cref="Player"/> from a vehicle that it's attached to
        /// </summary>
        public void Dismount()
        {
            if (VehicleGuid != 0u)
            {
                Vehicle vehicle = GetVisible<Vehicle>(VehicleGuid);
                vehicle.PassengerRemove(this);
                if (vehicle is Mount mount)
                {
                    if(mount.castingId != 0)
                    {
                        CancelEffect(mount.castingId);
                    }
                }
            }
        }

        /// <summary>
        /// Remove all entities associated with the <see cref="Player"/>
        /// </summary>
        private void DestroyDependents()
        {
            // vehicle will be removed if player is the last passenger
            if (VehicleGuid != 0u)
                Dismount();

            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.RemoveFromMap();
                VanityPetGuid = null;
            }

            // TODO: Remove pets, scanbots
        }

        public void DestroyPet()
        {
            // enqueue removal of existing vanity pet if summoned
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.RemoveFromMap();
                VanityPetGuid = null;
            }
        }

        public void SetPetFollowing(bool isPetFollowing)
        {
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.SetIsFollowingPlayer(isPetFollowing);
            }
        }

        public void SetPetFacingPlayer(bool isPetFacingPlayer)
        {
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.SetIsFacingPlayer(isPetFacingPlayer);
            }
        }

        public void SetPetFollowingOnSide(bool isPetFollowingOnSide)
        {
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.SetFollowingOnSide(isPetFollowingOnSide);
            }
        }

        public void SetPetFollowDistance(float dist)
        {
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.SetFollowDistance(dist);
            }
        }
        public void SetPetFollowRecalculateDistance(float dist)
        {
            if (VanityPetGuid != null)
            {
                VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
                pet?.SetFollowFollowMinRecalculateDistance(dist);
            }
        }

        public Creature2Entry VanityPetCreatureEntry()
        {
            VanityPet pet = GetVisible<VanityPet>(VanityPetGuid.Value);
            return pet?.Creature;
        }

        /// <summary>
        /// Returns the time in seconds that has past since the last <see cref="Player"/> save.
        /// </summary>
        public double GetTimeSinceLastSave()
        {
            return SaveDuration - saveTimer.Time;
        }

        /// <summary>
        /// Return <see cref="Disposition"/> between <see cref="Player"/> and <see cref="Faction"/>.
        /// </summary>
        public override Disposition GetDispositionTo(Faction factionId, bool primary = true)
        {
            FactionNode targetFaction = FactionManager.Instance.GetFaction(factionId);
            if (targetFaction == null)
                throw new ArgumentException($"Invalid faction {factionId}!");

            // find disposition based on reputation level
            Disposition? dispositionFromReputation = GetDispositionFromReputation(targetFaction);
            if (dispositionFromReputation.HasValue)
                return dispositionFromReputation.Value;

            return base.GetDispositionTo(factionId, primary);
        }

        private Disposition? GetDispositionFromReputation(FactionNode node)
        {
            if (node == null)
                return null;

            // check if current node has required reputation
            Reputation.Reputation reputation = ReputationManager.GetReputation(node.FactionId);
            if (reputation != null)
                return FactionNode.GetDisposition(FactionNode.GetFactionLevel(reputation.Amount));

            // check if parent node has required reputation
            return GetDispositionFromReputation(node.Parent);
        }

        /// <summary>
        /// Add a new <see cref="CharacterFlag"/>.
        /// </summary>
        public void SetFlag(CharacterFlag flag)
        {
            Flags |= flag;
            SendCharacterFlagsUpdated();
        }

        /// <summary>
        /// Remove an existing <see cref="CharacterFlag"/>.
        /// </summary>
        public void RemoveFlag(CharacterFlag flag)
        {
            Flags &= ~flag;
            SendCharacterFlagsUpdated();
        }

        /// <summary>
        /// Returns if supplied <see cref="CharacterFlag"/> exists.
        /// </summary>
        public bool HasFlag(CharacterFlag flag)
        {
            return (Flags & flag) != 0;
        }

        /// <summary>
        /// Send <see cref="ServerCharacterFlagsUpdated"/> to client.
        /// </summary>
        public void SendCharacterFlagsUpdated()
        {
            Session.EnqueueMessageEncrypted(new ServerCharacterFlagsUpdated
            {
                Flags = flags
            });
        }
    }
}
