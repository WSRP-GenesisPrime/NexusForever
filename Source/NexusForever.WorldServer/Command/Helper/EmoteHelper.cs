using NexusForever.WorldServer.Game.Entity.Static;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Command.Helper
{
    public abstract class EmoteHelper
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// (GENESIS PRIME) Dictionary of legal emotes and their IDs.
        /// </summary>
        private static readonly Dictionary<String, uint> EmoteLibrary = new Dictionary<String, uint>()
        {
            { "chairsit", 289 },
            { "chairsit2", 288 },
            { "channeling", 280 },
            { "channeling2", 231 },
            { "channeling3", 417 },
            { "combatloop", 199 },
            { "dazed", 59 },
            { "dazedfloat", 259 },
            { "deadfloat", 134 },
            { "deadfloat2", 261 },
            { "dead", 46 },
            { "dead2", 184 },
            { "dead3", 185 },
            { "dead4", 186 },
            { "dead5", 187 },
            { "dominionpose", 291 },
            { "exilepose", 290 },
            { "falling", 214 },
            { "floating", 216 },
            { "holdobject", 83 },
            { "knockdown", 158 },
            { "laser", 96 },
            { "lounge", 425 },
            { "mount", 267 },
            { "pistolfire", 371 },
            { "readyclaws", 86 },
            { "readycombat", 43 },
            { "readycombatfloat", 269 },
            { "readylauncher", 266 },
            { "readypistols", 54 },
            { "readyrifle", 39 },
            { "readysword", 85 },
            { "shiver", 427 },
            { "staffchannel", 249 },
            { "staffraise", 155 },
            { "stealth", 156 },
            { "swordblock", 232 },
            { "talking", 97 },
            { "taxisit", 263 },
            { "tiedup", 102 },
            { "tpose", 203 },
            { "use", 42 },
            { "use2", 35 },
            { "wounded", 98 },
            { "wounded2", 99 },
            { "wounded3", 100 },
            { "wounded4", 101 },
            //Emotes added by Archive Update
            { "lounge2", 433 },
            { "lounge3", 434 },
            { "coffin", 435 },
            { "drakenlounge", 436 },
            { "drakenlounge2", 437 },
            { "drakenlounge3", 438 },
            { "drakenlounge4", 439 },
            { "drakenlounge5", 440 },
            { "drakenlounge6", 441 },
            { "drakenlounge7", 442 },
            { "drakenlounge8", 443 },
            { "drakenlounge9", 444 },
            { "drakenlounge10", 445 },
            { "drakenthrone", 446 },
            { "channeling4", 447 },
            { "2hstrike", 448 },
            { "combatloop2", 449 },
            { "npcstance", 450 },
            { "npcstance2", 451 },
            { "stalkerstance", 452 },
            { "stalkerstance2", 453 },
            { "bladedance", 454 },
            { "meditate", 455 },
            { "readyesper", 456 },
            { "channeling5", 457 },
            { "channeling6", 458 },
            { "channeling7", 459 },
            { "channeling8", 460 },
            { "rapidfire", 461 },
            { "rapidfire2", 462 },
            { "chargeshot", 463 },
            { "readyresos", 464 },
            { "resofire", 465 },
            { "resofire2", 466 },
            { "resocharge", 467 },
            { "resofire3", 468 },
            { "readycombat2", 469 },
            { "readystaff", 470 }
        };

        /// <summary>
        /// (GENESIS PRIME) Dictionary that represents exclusion of certain player races from using certain emotes.
        /// </summary>
        private static readonly Dictionary<String, List<uint>> EmoteExclusionLibrary = new Dictionary<String, List<uint>>()
        {
            { "dominionpose", new List<uint>()
                {
                    3, 4, 16
                }
            },
            { "exilepose", new List<uint>()
                {
                    5, 12, 13
                }
            },
            { "staffchannel", new List<uint>()
                {
                    3, 12, 16
                }
            },
            { "staffraise", new List<uint>()
                {
                    3, 12, 16
                }
            },
            { "stealth", new List<uint>()
                {
                    3, 13
                }
            },
            { "pistolfire", new List<uint>()
                {
                    3, 12
                }
            },
            { "channeling3", new List<uint>()
                {
                    3, 12
                }
            },
            { "channeling2", new List<uint>()
                {
                    4
                }
            },
            { "swordblock", new List<uint>()
                {
                    4
                }
            },
            //Added by Archive Update
            { "drakenlounge", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge2", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge3", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge4", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge5", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge6", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge7", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge8", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge9", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "drakenlounge10", new List<uint>()
                {
                    1, 2, 3, 4, 12, 13, 16
                }
            },
            { "2hstrike", new List<uint>()
                {
                    4
                }
            },
            { "combatloop2", new List<uint>()
                {
                    4
                }
            },
            { "stalkerstance", new List<uint>()
                {
                    3
                }
            },
            { "stalkerstance", new List<uint>()
                {
                    3
                }
            },
            { "stalkerstance2", new List<uint>()
                {
                    3
                }
            },
            { "bladedance", new List<uint>()
                {
                    3, 12, 16
                }
            },
            { "meditate", new List<uint>()
                {
                    3, 12, 16
                }
            },
            { "channeling5", new List<uint>()
                {
                    3, 12, 16
                }
            },
            { "channeling6", new List<uint>()
                {
                    3, 12, 16
                }
            },
            { "channeling7", new List<uint>()
                {
                    3, 12, 16
                }
            },
            { "rapidfire", new List<uint>()
                {
                    3, 12
                }
            },
            { "rapidfire2", new List<uint>()
                {
                    3, 12
                }
            },
            { "chargeshot", new List<uint>()
                {
                    3, 12
                }
            },
            { "readyresos", new List<uint>()
                {
                    4, 5
                }
            },
            { "resofire", new List<uint>()
                {
                    4, 5
                }
            },
            { "resofire2", new List<uint>()
                {
                    4, 5
                }
            },
            { "resocharge", new List<uint>()
                {
                    4, 5
                }
            },
            { "resofire3", new List<uint>()
                {
                    4, 5
                }
            },
            { "readystaff", new List<uint>()
                {
                    3, 12
                }
            }
        };

        /// <summary>
        /// (GENESIS PRIME) Dictionary that represents exclusion of player character sexes from using certain emotes.
        /// </summary>
        private static readonly Dictionary<String, List<uint>> EmoteSexExclusionLibrary = new Dictionary<String, List<uint>>()
        {
            { "dead5", new List<uint>()
                {
                    0
                }
            },
            //Added by Archive Update
            { "drakenlounge7", new List<uint>()
                {
                    0
                }
            },
            { "drakenlounge8", new List<uint>()
                {
                    0
                }
            },
            { "drakenlounge9", new List<uint>()
                {
                    0
                }
            },
            { "drakenlounge10", new List<uint>()
                {
                    0
                }
            },
            { "drakenthrone", new List<uint>()
                {
                    1
                }
            },
        };

        /// <summary>
        /// (GENESIS PRIME) Returns whether the given emote and race combo is a no-go.
        /// </summary>
        public static bool IsEmoteAllowedByRace(string emoteName, uint playerRaceID)
        {
            if (EmoteExclusionLibrary.TryGetValue(emoteName, out List<uint> exclusionList))
            {
                return !(exclusionList.Contains(playerRaceID));
            }
            return true;
        }
        public static bool IsEmoteAllowedBySex(string emoteName, uint playerRaceID)
        {
            if (EmoteSexExclusionLibrary.TryGetValue(emoteName, out List<uint> exclusionList))
            {
                return !(exclusionList.Contains(playerRaceID));
            }
            return true;
        }

        /// <summary>
        /// (GENESIS PRIME) Get the ID of a legal emote.
        /// </summary>
        public static uint? GetEmoteId(string emoteName)
        {
            if (EmoteLibrary.TryGetValue(emoteName, out uint returnEmoteId))
            {
                return returnEmoteId;
            }
            return null;
        }

        /// <summary>
        /// (GENESIS PRIME) Get a list of valid emotes.
        /// </summary>
        public static List<string> GetEmoteList(uint playerRaceID)
        {
            return EmoteLibrary.Keys.Where(e => IsEmoteAllowedByRace(e, playerRaceID) && IsEmoteAllowedBySex(e, playerRaceID)).ToList();
        }
    }
}
