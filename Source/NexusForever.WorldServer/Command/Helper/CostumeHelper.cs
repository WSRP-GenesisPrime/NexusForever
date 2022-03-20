using NexusForever.WorldServer.Game.Entity.Static;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Command.Helper
{
    public abstract class CostumeHelper
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// (GENESIS PRIME) 2D Dictionary of legal item display type/variant combinations.
        /// </summary>
        private static readonly Dictionary<string, ushort> VariantsWeaponHammer = new Dictionary<string, ushort>()
        {
            { "generic", 632 },
            { "generic2", 572 },
            { "generic3", 7156 },
            { "grund", 633 },
            { "osun", 2245 },
            { "osun2", 7157 },
            { "osun3", 7952 },
            { "phage", 6597 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponMace = new Dictionary<string, ushort>()
        {
            { "moodie", 8639 },
            { "murgh", 6970 },
            { "murgh2", 6969 },
            { "pell", 2092 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponMisc = new Dictionary<string, ushort>()
        {
            { "bone", 602 },
            { "flashlight", 1588 },
            { "mug", 1343 },
            { "plank", 3400 },
            { "shovel", 684 },
            { "sandwich", 8051 },
            { "drumstick", 7829 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponRifle = new Dictionary<string, ushort>()
        {
            { "bazooka", 5393 },
            { "crossbow", 4869 },
            { "fishcannon", 3049 },
            { "flamer", 2332 },
            { "flashlight", 4793 },
            { "freeze", 6588 },
            { "lasersaw", 3161 },
            { "pickaxe", 1946 },
            { "plasma", 149 },
            { "relicblaster", 3138 },
            { "sniper", 21 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponScythe = new Dictionary<string, ushort>()
        {
            { "generic", 184 },
            { "generic2", 588 },
            { "generic3", 589 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponStaff = new Dictionary<string, ushort>()
        {
            { "fan", 8029 },
            { "generic", 464 },
            { "generic2", 465 },
            { "generic3", 144 },
            { "generic4", 1611 },
            { "generic5", 1612 },
            { "generic6", 1613 },
            { "generic7", 1614 },
            { "generic8", 1615 },
            { "ikthian", 8019 },
            { "laveka", 7837 },
            { "lopp", 595 },
            { "osun", 2333 },
            { "osun2", 3071 },
            { "osunglaive", 7951 },
            { "osunglaive2", 7816 },
            { "pell", 2090 },
            { "pell2", 2091 },
            { "skeech", 79 },
            { "skeech2", 7290 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponSword1H = new Dictionary<string, ushort>()
        {
            { "cross", 1 },
            { "falkrin", 6972 },
            { "generic", 612 },
            { "generic2", 145 },
            { "pell", 2089 },
            { "torohawk", 2386 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponSword2H = new Dictionary<string, ushort>()
        {
            { "corrupted", 7018 },
            { "exile", 6968 },
            { "falkrin", 2093 },
            { "falkrin2", 6967 },
            { "generic", 30 },
            { "generic2", 12 }
        };
        private static readonly Dictionary<string, ushort> VariantsWeaponWrench = new Dictionary<string, ushort>()
        {
            { "small", 39 },
            { "big", 147 },
            { "ikthian", 1617 }
        };
        private static readonly Dictionary<string, (Dictionary<string, ushort>, ushort)> ItemDisplayVariantLibrary = new Dictionary<string, (Dictionary<string, ushort>, ushort)>()
        {
            {
                "hammer",
                (VariantsWeaponHammer, 632)
            },
            {
                "mace",
                (VariantsWeaponMace, 2092)
            },
            {
                "misc",
                (VariantsWeaponMisc, 684)
            },
            {
                "rifle",
                (VariantsWeaponRifle, 149)
            },
            {
                "scythe",
                (VariantsWeaponScythe, 184)
            },
            {
                "staff",
                (VariantsWeaponStaff, 464)
            },
            {
                "sword1h",
                (VariantsWeaponSword1H, 612)
            },
            {
                "sword2h",
                (VariantsWeaponSword2H, 12)
            },
            {
                "wrench",
                (VariantsWeaponWrench, 147)
            }
        };

        private static readonly Dictionary<string, ushort> VariantsBodyType = new Dictionary<string, ushort>()
        {
            { "default", 7277 },
            { "skinny2", 7282 },
            { "skinny1", 7280 },
            { "buff1", 7285 },
            { "buff2", 7289 },
            { "chesty", 7792 },
            { "slender", 7793 },
            { "thick", 8422 },
            { "legs", 7795 },
        };

        /// <summary>
        /// (GENESIS PRIME) Get the ID of a legal item display type/variant combination for Costume override commands.
        /// Returns a nullable ushort. Null means the item display could not be found.
        /// </summary>
        public static ushort? GetItemDisplayIdFromType(ItemSlot slot, string itemType, string itemVariant)
        {
            if (slot == ItemSlot.WeaponPrimary) // or other type with categories, I guess
            {
                if (string.IsNullOrWhiteSpace(itemType))
                {
                    return null;
                }

                // get the dictionary for the category
                if (ItemDisplayVariantLibrary.TryGetValue(itemType.ToLower(), out (Dictionary<string, ushort>, ushort) entry))
                {
                    if (string.IsNullOrWhiteSpace(itemVariant))
                    {
                        return entry.Item2;
                    }
                    else
                    {
                        // get the item display ID corresponding to the variant
                        if (entry.Item1.TryGetValue(itemVariant.ToLower(), out ushort returnItemDisplayId))
                        {
                            return returnItemDisplayId;
                        }
                    }
                }
            }
            else if (slot == ItemSlot.BodyType)
            {
                if(VariantsBodyType.TryGetValue(itemType.ToLower(), out ushort val))
                {
                    return val;
                }
            }
            return null;
        }

        /// <summary>
        /// (GENESIS PRIME) Get a list of valid item display types.
        /// </summary>
        public static List<string> getItemTypeList(ItemSlot slot)
        {
            if (slot == ItemSlot.WeaponPrimary)
            {
                return ItemDisplayVariantLibrary.Keys.ToList();
            }
            if (slot == ItemSlot.BodyType)
            {
                return VariantsBodyType.Keys.ToList();
            }
            return null;
        }

        /// <summary>
        /// (GENESIS PRIME) Get a list of valid item display variants for a type.
        /// </summary>
        public static List<string> getItemsForType(ItemSlot slot, string itemType)
        {
            if(slot != ItemSlot.WeaponPrimary)
            {
                return null;
            }
            (Dictionary<string, ushort>, ushort) itemDisplaySubLibrary;
            // get the item display type-specific dictionary
            if (ItemDisplayVariantLibrary.TryGetValue(itemType, out itemDisplaySubLibrary))
            {
                return new List<string>(itemDisplaySubLibrary.Item1.Keys);
            }
            return null;
        }
    }
}
