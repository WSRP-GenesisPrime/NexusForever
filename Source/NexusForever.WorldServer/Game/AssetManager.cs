using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reflection;
using NexusForever.Database.Character.Model;
using NexusForever.Database.World.Model;
using NexusForever.Shared;
using NexusForever.Shared.Database;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game.Reputation.Static;
using NexusForever.WorldServer.Game.Static;

namespace NexusForever.WorldServer.Game
{
    public sealed class AssetManager : Singleton<AssetManager>
    {
        public static ImmutableDictionary<InventoryLocation, uint> InventoryLocationCapacities { get; private set; }

        /// <summary>
        /// Id to be assigned to the next created character.
        /// </summary>
        public ulong NextCharacterId => nextCharacterId++;

        /// <summary>
        /// Id to be assigned to the next created mail.
        /// </summary>
        public ulong NextMailId => nextMailId++;

        private ulong nextCharacterId;
        private ulong nextMailId;

        private ImmutableDictionary<(Race, Faction, CharacterCreationStart), Location> characterCreationData;
        private ImmutableDictionary<uint, ImmutableList<CharacterCustomizationEntry>> characterCustomisations;
        private ImmutableList<PropertyValue> characterBaseProperties;
        private ImmutableDictionary<Class, ImmutableList<PropertyValue>> characterClassBaseProperties;

        private ImmutableDictionary<uint, ImmutableList<ItemDisplaySourceEntryEntry>> itemDisplaySourcesEntry;
        private ImmutableDictionary<uint /*item2CategoryId*/, float /*modifier*/> itemArmorModifiers;

        private ImmutableDictionary</*zoneId*/uint, /*tutorialId*/uint> zoneTutorials;
        private ImmutableDictionary</*creatureId*/uint, /*targetGroupIds*/ImmutableList<uint>> creatureAssociatedTargetGroups;

        private ImmutableDictionary<AccountTier, ImmutableList<RewardPropertyPremiumModifierEntry>> rewardPropertiesByTier;

        private AssetManager()
        {
        }

        public void Initialise()
        {
            nextCharacterId = DatabaseManager.Instance.CharacterDatabase.GetNextCharacterId() + 1ul;
            nextMailId      = DatabaseManager.Instance.CharacterDatabase.GetNextMailId() + 1ul;

            CacheCharacterCreate();
            CacheCharacterCustomisations();
            CacheCharacterBaseProperties();
            CacheCharacterClassBaseProperties();
            CacheInventoryBagCapacities();
            CacheItemDisplaySourceEntries();
            CacheItemArmorModifiers();
            CacheTutorials();
            CacheCreatureTargetGroups();
            CacheRewardPropertiesByTier();
        }

        private void CacheCharacterCreate()
        {
            var entries = ImmutableDictionary.CreateBuilder<(Race, Faction, CharacterCreationStart), Location>();
            foreach (CharacterCreateModel model in DatabaseManager.Instance.CharacterDatabase.GetCharacterCreationData())
            {
                entries.Add(((Race)model.Race, (Faction)model.Faction, (CharacterCreationStart)model.CreationStart), new Location
                (
                    GameTableManager.Instance.World.GetEntry(model.WorldId),
                    new Vector3
                    {
                        X = model.X,
                        Y = model.Y,
                        Z = model.Z
                    },
                    new Vector3
                    {
                        X = model.Rx,
                        Y = model.Ry,
                        Z = model.Rz
                    }
                ));
            }

            characterCreationData = entries.ToImmutable();
        }

        private void CacheCharacterCustomisations()
        {
            var entries = new Dictionary<uint, List<CharacterCustomizationEntry>>();
            foreach (CharacterCustomizationEntry entry in GameTableManager.Instance.CharacterCustomization.Entries)
            {
                uint primaryKey;
                if (entry.CharacterCustomizationLabelId00 == 0 && entry.CharacterCustomizationLabelId01 > 0)
                    primaryKey = (entry.Value01 << 24) | (entry.CharacterCustomizationLabelId01 << 16) | (entry.Gender << 8) | entry.RaceId;
                else
                    primaryKey = (entry.Value00 << 24) | (entry.CharacterCustomizationLabelId00 << 16) | (entry.Gender << 8) | entry.RaceId;

                if (!entries.ContainsKey(primaryKey))
                    entries.Add(primaryKey, new List<CharacterCustomizationEntry>());

                entries[primaryKey].Add(entry);
            }

            characterCustomisations = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }

        private void CacheCharacterBaseProperties()
        {
            var entries = ImmutableList.CreateBuilder<PropertyValue>();
            entries.Add(new PropertyValue(Property.Strength, 0, 0));
            entries.Add(new PropertyValue(Property.Dexterity, 0, 0));
            entries.Add(new PropertyValue(Property.Technology, 0, 0));
            entries.Add(new PropertyValue(Property.Magic, 0, 0));
            entries.Add(new PropertyValue(Property.Wisdom, 0, 0));
            entries.Add(new PropertyValue(Property.BaseHealth, 200, 200));
            entries.Add(new PropertyValue(Property.ResourceMax0, 500, 500));
            entries.Add(new PropertyValue(Property.ResourceRegenMultiplier0, 0.0225f, 0.0225f));
            entries.Add(new PropertyValue(Property.AssaultRating, 18, 18));
            entries.Add(new PropertyValue(Property.SupportRating, 18, 18));
            entries.Add(new PropertyValue(Property.ResourceMax7, 200, 200));
            entries.Add(new PropertyValue(Property.ResourceRegenMultiplier7, 0.045f, 0.045f));
            entries.Add(new PropertyValue(Property.MoveSpeedMultiplier, 1, 1));
            entries.Add(new PropertyValue(Property.BaseAvoidChance, 0.05f, 0.05f));
            entries.Add(new PropertyValue(Property.BaseCritChance, 0.05f, 0.05f));
            entries.Add(new PropertyValue(Property.BaseFocusRecoveryInCombat, 0, 0));
            entries.Add(new PropertyValue(Property.BaseFocusRecoveryOutofCombat, 0, 0));
            entries.Add(new PropertyValue(Property.FrictionMax, 1f, 1f));
            entries.Add(new PropertyValue(Property.BaseMultiHitAmount, 0.3f, 0.3f));
            entries.Add(new PropertyValue(Property.JumpHeight, 5f, 5f));
            entries.Add(new PropertyValue(Property.GravityMultiplier, 0.8f, 0.8f));
            entries.Add(new PropertyValue(Property.DamageTakenOffsetPhysical, 1, 1));
            entries.Add(new PropertyValue(Property.DamageTakenOffsetTech, 1, 1));
            entries.Add(new PropertyValue(Property.DamageTakenOffsetMagic, 1, 1));
            entries.Add(new PropertyValue(Property.BaseMultiHitChance, 0.05f, 0.05f));
            entries.Add(new PropertyValue(Property.BaseDamageReflectAmount, 0.05f, 0.05f));
            entries.Add(new PropertyValue(Property.SlowFallMultiplier, 1f, 1f));
            entries.Add(new PropertyValue(Property.MountSpeedMultiplier, 2, 2));
            entries.Add(new PropertyValue(Property.BaseGlanceAmount, 0.3f, 0.3f));

            characterBaseProperties = entries.ToImmutable();
        }

        private void CacheCharacterClassBaseProperties()
        {
            ImmutableDictionary<Class, ImmutableList<PropertyValue>>.Builder entries = ImmutableDictionary.CreateBuilder<Class, ImmutableList<PropertyValue>>();

            { // Warrior
                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                propertyList.Add(new PropertyValue(Property.ResourceMax1, 1000, 1000));
                propertyList.Add(new PropertyValue(Property.ResourceRegenMultiplier1, 1, 1));
                ImmutableList<PropertyValue> classProperties = propertyList.ToImmutable();
                entries.Add(Class.Warrior, classProperties);
            }

            { // Engineer
                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                propertyList.Add(new PropertyValue(Property.ResourceMax1, 100, 100));
                ImmutableList<PropertyValue> classProperties = propertyList.ToImmutable();
                entries.Add(Class.Engineer, classProperties);
            }

            { // Best class
                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                propertyList.Add(new PropertyValue(Property.ResourceMax1, 5, 5));
                propertyList.Add(new PropertyValue(Property.BaseFocusPool, 1000, 1000));
                propertyList.Add(new PropertyValue(Property.BaseFocusRecoveryInCombat, 0.005f, 0.005f));
                propertyList.Add(new PropertyValue(Property.BaseFocusRecoveryOutofCombat, 0.02f, 0.02f));
                ImmutableList<PropertyValue> classProperties = propertyList.ToImmutable();
                entries.Add(Class.Esper, classProperties);
            }

            { // Medic
                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                propertyList.Add(new PropertyValue(Property.ResourceMax1, 4, 4));
                propertyList.Add(new PropertyValue(Property.BaseFocusPool, 1000, 1000));
                propertyList.Add(new PropertyValue(Property.BaseFocusRecoveryInCombat, 0.005f, 0.005f));
                propertyList.Add(new PropertyValue(Property.BaseFocusRecoveryOutofCombat, 0.02f, 0.02f));
                ImmutableList<PropertyValue> classProperties = propertyList.ToImmutable();
                entries.Add(Class.Medic, classProperties);
            }

            { // Stalker
                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                propertyList.Add(new PropertyValue(Property.ResourceMax3, 100, 100));
                propertyList.Add(new PropertyValue(Property.ResourceRegenMultiplier3, 0.035f, 0.035f));
                ImmutableList<PropertyValue> classProperties = propertyList.ToImmutable();
                entries.Add(Class.Stalker, classProperties);
            }

            { // Spellslinger
                ImmutableList<PropertyValue>.Builder propertyList = ImmutableList.CreateBuilder<PropertyValue>();
                propertyList.Add(new PropertyValue(Property.ResourceMax4, 100, 100));
                propertyList.Add(new PropertyValue(Property.BaseFocusPool, 1000, 1000));
                propertyList.Add(new PropertyValue(Property.BaseFocusRecoveryInCombat, 0.005f, 0.005f));
                propertyList.Add(new PropertyValue(Property.BaseFocusRecoveryOutofCombat, 0.02f, 0.02f));
                ImmutableList<PropertyValue> classProperties = propertyList.ToImmutable();
                entries.Add(Class.Spellslinger, classProperties);
            }

            characterClassBaseProperties = entries.ToImmutable();
        }

        public void CacheInventoryBagCapacities()
        {
            var entries = new Dictionary<InventoryLocation, uint>();
            foreach (FieldInfo field in typeof(InventoryLocation).GetFields())
            {
                foreach (InventoryLocationAttribute attribute in field.GetCustomAttributes<InventoryLocationAttribute>())
                {
                    InventoryLocation location = (InventoryLocation)field.GetValue(null);
                    entries.Add(location, attribute.DefaultCapacity);
                }
            }

            InventoryLocationCapacities = entries.ToImmutableDictionary();
        }

        private void CacheItemDisplaySourceEntries()
        {
            var entries = new Dictionary<uint, List<ItemDisplaySourceEntryEntry>>();
            foreach (ItemDisplaySourceEntryEntry entry in GameTableManager.Instance.ItemDisplaySourceEntry.Entries)
            {
                if (!entries.ContainsKey(entry.ItemSourceId))
                    entries.Add(entry.ItemSourceId, new List<ItemDisplaySourceEntryEntry>());

                entries[entry.ItemSourceId].Add(entry);
            }

            itemDisplaySourcesEntry = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }

        private void CacheTutorials()
        {
            var zoneEntries =  ImmutableDictionary.CreateBuilder<uint, uint>();
            foreach (TutorialModel tutorial in DatabaseManager.Instance.WorldDatabase.GetTutorialTriggers())
            {
                if (tutorial.TriggerId == 0) // Don't add Tutorials with no trigger ID
                    continue;

                if (tutorial.Type == 29 && !zoneEntries.ContainsKey(tutorial.TriggerId))
                    zoneEntries.Add(tutorial.TriggerId, tutorial.Id);
            }

            zoneTutorials = zoneEntries.ToImmutable();
        }

        private void CacheCreatureTargetGroups()
        {
            var entries = ImmutableDictionary.CreateBuilder<uint, List<uint>>();
            foreach (TargetGroupEntry entry in GameTableManager.Instance.TargetGroup.Entries)
            {
                if ((TargetGroupType)entry.Type != TargetGroupType.CreatureIdGroup)
                    continue;

                foreach (uint creatureId in entry.DataEntries)
                {
                    if (!entries.ContainsKey(creatureId))
                        entries.Add(creatureId, new List<uint>());

                    entries[creatureId].Add(entry.Id);
                }
            }

            creatureAssociatedTargetGroups = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }
        
        private void CacheItemArmorModifiers()
        {
            var armorMods = ImmutableDictionary.CreateBuilder<uint, float>();
            foreach (Item2CategoryEntry entry in GameTableManager.Instance.Item2Category.Entries.Where(i => i.Item2FamilyId == 1))
                armorMods.Add(entry.Id, entry.ArmorModifier);

            itemArmorModifiers = armorMods.ToImmutable();
        }

        private void CacheRewardPropertiesByTier()
        {
            // VIP was intended to be used in China from what I can see, you can force the VIP premium system in the client with the China game mode parameter
            // not supported as the system was unfinished
            IEnumerable<RewardPropertyPremiumModifierEntry> hybridEntries = GameTableManager.Instance
                .RewardPropertyPremiumModifier.Entries
                .Where(e => (PremiumSystem)e.PremiumSystemEnum == PremiumSystem.Hybrid)
                .ToList();

            // base reward properties are determined by current account tier and lower if fall through flag is set
            rewardPropertiesByTier = hybridEntries
                .Select(e => e.Tier)
                .Distinct()
                .ToImmutableDictionary(k => (AccountTier)k, k => hybridEntries
                    .Where(r => r.Tier == k)
                    .Concat(hybridEntries
                        .Where(r => r.Tier < k && ((RewardPropertyPremiumModiferFlags)r.Flags & RewardPropertyPremiumModiferFlags.FallThrough) != 0))
                    .ToImmutableList());
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="CharacterCustomizationEntry"/>'s for the supplied race, sex, label and value.
        /// </summary>
        public ImmutableList<CharacterCustomizationEntry> GetPrimaryCharacterCustomisation(uint race, uint sex, uint label, uint value)
        {
            uint key = (value << 24) | (label << 16) | (sex << 8) | race;
            return characterCustomisations.TryGetValue(key, out ImmutableList<CharacterCustomizationEntry> entries) ? entries : null;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList[T]"/> containing all base <see cref="PropertyValue"/> for any character
        /// </summary>
        public ImmutableList<PropertyValue> GetCharacterBaseProperties()
        {
            return characterBaseProperties;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList[T]"/> containing all base <see cref="PropertyValue"/> for a character class
        /// </summary>
        public ImmutableList<PropertyValue> GetCharacterClassBaseProperties(Class @class)
        {
            return characterClassBaseProperties.TryGetValue(@class, out ImmutableList<PropertyValue> propertyValues) ? propertyValues : null;
        }
        
        /// <summary>
        /// Returns matching <see cref="CharacterCustomizationEntry"/> given input parameters
        /// </summary>
        public IEnumerable<CharacterCustomizationEntry> GetCharacterCustomisation(Dictionary<uint, uint> customisations, uint race, uint sex, uint primaryLabel, uint primaryValue)
        {
            ImmutableList<CharacterCustomizationEntry> entries = GetPrimaryCharacterCustomisation(race, sex, primaryLabel, primaryValue);
            if (entries == null)
                return Enumerable.Empty<CharacterCustomizationEntry>();

            List<CharacterCustomizationEntry> customizationEntries = new List<CharacterCustomizationEntry>();

            // Customisation has multiple results, filter with a non-zero secondary KvP.
            List<CharacterCustomizationEntry> primaryEntries = entries.Where(e => e.CharacterCustomizationLabelId01 != 0).ToList();
            if (primaryEntries.Count > 0)
            {
                // This will check all entries where there is a primary AND secondary KvP.
                foreach (CharacterCustomizationEntry customizationEntry in primaryEntries)
                {
                    // Missing primary KvP in table, skipping.
                    if (customizationEntry.CharacterCustomizationLabelId00 == 0)
                        continue;

                    // Secondary KvP not found in customisation list, skipping.
                    if (!customisations.ContainsKey(customizationEntry.CharacterCustomizationLabelId01))
                        continue;

                    // Returning match found for primary KvP and secondary KvP
                    if (customisations[customizationEntry.CharacterCustomizationLabelId01] == customizationEntry.Value01)
                        customizationEntries.Add(customizationEntry);
                }

                // Return the matching value when the primary KvP matching the table's secondary KvP
                CharacterCustomizationEntry entry = entries.FirstOrDefault(e => e.CharacterCustomizationLabelId01 == primaryLabel && e.Value01 == primaryValue);
                if (entry != null)
                    customizationEntries.Add(entry);
            }
            if (customizationEntries.Count == 0)
            {
                // Return the matching value when the primary KvP matches the table's primary KvP, and no secondary KvP is present.
                CharacterCustomizationEntry entry = entries.FirstOrDefault(e => e.CharacterCustomizationLabelId00 == primaryLabel && e.Value00 == primaryValue);
                if (entry != null)
                    customizationEntries.Add(entry);
                else
                {
                    entry = entries.Single(e => e.CharacterCustomizationLabelId01 == 0 && e.Value01 == 0);
                    if (entry != null)
                        customizationEntries.Add(entry);
                }
            }

            return customizationEntries;
        }
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="ItemDisplaySourceEntryEntry"/>'s for the supplied itemSource.
        /// </summary>
        public ImmutableList<ItemDisplaySourceEntryEntry> GetItemDisplaySource(uint itemSource)
        {
            return itemDisplaySourcesEntry.TryGetValue(itemSource, out ImmutableList<ItemDisplaySourceEntryEntry> entries) ? entries : null;
        }

        /// <summary>
        /// Returns a Tutorial ID if it's found in the Zone Tutorials cache
        /// </summary>
        public uint GetTutorialIdForZone(uint zoneId)
        {
            return zoneTutorials.TryGetValue(zoneId, out uint tutorialId) ? tutorialId : 0;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all TargetGroup ID's associated with the creatureId.
        /// </summary>
        public ImmutableList<uint> GetTargetGroupsForCreatureId(uint creatureId)
        {
            return creatureAssociatedTargetGroups.TryGetValue(creatureId, out ImmutableList<uint> entries) ? entries : null;
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="RewardPropertyPremiumModifierEntry"/> for the given <see cref="AccountTier"/>.
        /// </summary>
        public ImmutableList<RewardPropertyPremiumModifierEntry> GetRewardPropertiesForTier(AccountTier tier)
        {
            return rewardPropertiesByTier.TryGetValue(tier, out ImmutableList<RewardPropertyPremiumModifierEntry> entries) ? entries : ImmutableList<RewardPropertyPremiumModifierEntry>.Empty;
        }

        /// <summary>
        /// Returns a <see cref="Location"/> describing the starting location for a given <see cref="Race"/>, <see cref="Faction"/> and Creation Type combination.
        /// </summary>
        public Location GetStartingLocation(Race race, Faction faction, CharacterCreationStart creationStart)
        {
            return characterCreationData.TryGetValue((race, faction, creationStart), out Location location) ? location : null;
        }
    }
}
