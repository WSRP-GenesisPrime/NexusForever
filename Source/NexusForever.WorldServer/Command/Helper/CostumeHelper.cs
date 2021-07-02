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
        private static readonly Dictionary<string, Dictionary<string, ushort>> ItemDisplayVariantLibrary = new Dictionary<string, Dictionary<string, ushort>>()
        {
            // Type: Hammer
            {
                "hammer",
                new Dictionary<string, ushort>()
                {
                    { "generic", 632 },
                    { "generic2", 572 },
                    { "generic3", 7156 },
                    { "grund", 633 },
                    { "osun", 2245 },
                    { "osun2", 7157 },
                    { "osun3", 7952 },
                    { "phage", 6597 }
                }
            },
            
            // Type: Mace
            {
                "mace",
                new Dictionary<string, ushort>()
                {
                    { "moodie", 8639 },
                    { "murgh", 6970 },
                    { "murgh2", 6969 },
                    { "pell", 2092 }
                }
            },

            // Type: Misc
            {
                "misc",
                new Dictionary<string, ushort>()
                {
                    { "bone", 602 },
                    { "flashlight", 1588 },
                    { "mug", 1343 },
                    { "plank", 3400 },
                    { "shovel", 684 }
                }
            },

            // Type: Rifle
            {
                "rifle",
                new Dictionary<string, ushort>()
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
                }
            },

            // Type: Scythe
            {
                "scythe",
                new Dictionary<string, ushort>()
                {
                    { "generic", 184 },
                    { "generic2", 588 },
                    { "generic3", 589 }
                }
            },

            // Type: Staff
            {
                "staff",
                new Dictionary<string, ushort>()
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
                }
            },

            // Type: Sword 1H
            {
                "sword1h",
                new Dictionary<string, ushort>()
                {
                    { "cross", 1 },
                    { "falkrin", 6972 },
                    { "generic", 612 },
                    { "generic2", 145 },
                    { "pell", 2089 },
                    { "torohawk", 2386 }
                }
            },

            // Type: Sword 2H
            {
                "sword2h",
                new Dictionary<string, ushort>()
                {
                    { "corrupted", 7018 },
                    { "exile", 6968 },
                    { "falkrin", 2093 },
                    { "falkrin2", 6967 },
                    { "generic", 30 },
                    { "generic2", 12 }
                }
            },

            // Type: Wrench
            {
                "wrench",
                new Dictionary<string, ushort>()
                {
                    { "small", 39 },
                    { "big", 147 },
                    { "ikthian", 1617 }
                }
            }
        };

        // Defaults for each type
        private static readonly Dictionary<string, ushort> ItemDisplayLibrary = new Dictionary<string, ushort>() {
            { "hammer", 632 },
            { "mace", 2092 },
            { "misc", 684 },
            { "rifle", 149 },
            { "scythe", 184 },
            { "staff", 464 },
            { "sword2h", 12 },
            { "sword1h", 612 },
            { "wrench", 147 }
        };

        /// <summary>
        /// (GENESIS PRIME) Get the ID of a legal item display type/variant combination for Costume override commands.
        /// Returns a nullable ushort. Null means the item display could not be found.
        /// </summary>
        public static ushort? GetItemDisplayIdFromType(string itemType, string itemVariant)
        {
            if (string.IsNullOrWhiteSpace(itemType))
            {
                return null;
            }
            Dictionary<string, ushort> itemDisplaySubLibrary;
            // get the item display type-specific dictionary

            if (string.IsNullOrWhiteSpace(itemVariant))
            {
                if (ItemDisplayLibrary.TryGetValue(itemType.ToLower(), out ushort itemDisplayId))
                {
                    return itemDisplayId;
                }
                return null;
            }

            if (ItemDisplayVariantLibrary.TryGetValue(itemType.ToLower(), out itemDisplaySubLibrary))
            {
                // get the item display ID corresponding to the variant
                if (itemDisplaySubLibrary.TryGetValue(itemVariant.ToLower(), out ushort returnItemDisplayId))
                {
                    return returnItemDisplayId;
                }
            }
            return null;
        }

        /// <summary>
        /// (GENESIS PRIME) Get a list of valid item display types.
        /// </summary>
        public static List<string> getItemTypeList()
        {
            return ItemDisplayVariantLibrary.Keys.ToList();
        }

        /// <summary>
        /// (GENESIS PRIME) Get a list of valid item display variants for a type.
        /// </summary>
        public static List<string> getCreatureVariantsForType(string itemType)
        {
            Dictionary<string, ushort> itemDisplaySubLibrary;
            // get the item display type-specific dictionary
            if (ItemDisplayVariantLibrary.TryGetValue(itemType, out itemDisplaySubLibrary))
            {
                return new List<string>(itemDisplaySubLibrary.Keys);
            }
            return null;
        }
    }
}
