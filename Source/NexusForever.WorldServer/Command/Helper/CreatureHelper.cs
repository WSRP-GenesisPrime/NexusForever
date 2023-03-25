using NexusForever.WorldServer.Command.Context;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Command.Helper
{
    public abstract class CreatureHelper
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// (GENESIS PRIME) 2D Dictionary of legal creature type/variant combinations.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, uint>> CreatureVariantLibrary = new Dictionary<string, Dictionary<string, uint>>() {
            // Type: Boss (Storyteller Only)
            { "boss", new Dictionary<string, uint>(){
                { "avatus", 60057 },
                { "bevorage", 61463 },
                { "dreadwatcher", 68254 },
                { "frostgale", 70446 },
                { "jackshade", 62681 },
                { "kundar", 70447 },
                { "laveka", 65997 },
                { "mordechai", 24489 },
                { "octog", 24486 },
                { "ohmna", 49395 },
                { "stormtalon", 17163 },
                { "zombiemordechai", 65800 },
                { "zombieoctog", 72758 },
                { "zombieoctog2", 75240 }
            }},
            
            // Type: Canimid
            { "canimid", new Dictionary<string, uint>(){
                { "augmented", 26973 },
                { "augmentedred", 21609 },
                { "augmentedred2", 32981 },
                { "brown", 19299 },
                { "grey", 32526 },
                { "red", 8120 },
                { "smoke", 61994 }
            }},
            
            // Type: Chompy
            { "chompy", new Dictionary<string, uint>(){
                { "black", 69490 },
                { "blue", 69489 },
                { "bluedarkspur", 69492 },
                { "dusty", 69486 },
                { "ginger", 69487 },
                { "orange", 69483 },
                { "strain", 69491 },
                { "tawny", 69485 },
                { "tawnydarkspur", 69488 },
                { "white", 71360 }
            }},
            
            // Type: Construct (Storyteller Only)
            { "construct", new Dictionary<string, uint>(){
                { "augmentor", 32956 },
                { "commander", 37375 },
                { "darkaugmentor", 65082 },
                { "darkcommander", 34806 },
                { "darkprobe", 34739 },
                { "darkprotector", 58289 },
                { "goldaugmentor", 63525 },
                { "goldcommander", 57165 },
                { "goldprotector", 70892 },
                { "phageaugmentor", 54935 },
                { "phagecommander", 20859 },
                { "phageprobe", 20863 },
                { "phageprotector", 20865 },
                { "probe", 45865 },
                { "protector", 20866 }
            }},
            
            // Type: Dagun
            { "dagun", new Dictionary<string, uint>(){
                { "black", 69407 },
                { "grim", 45867 },
                { "purple", 69411 },
                { "silver", 69410 },
                { "spacefaring", 69409 },
                { "strain", 69408 },
                { "white", 73296 }
            }},
            
            // Type: Dawngrazer
            { "dawngrazer", new Dictionary<string, uint>(){
                { "blue", 69404 },
                { "brown", 69401 },
                { "grey", 69402 },
                { "strain", 69405 },
                { "tan", 69403 },
                { "white", 69027 },
                { "zebra", 69406 }
            }},
            
            // Type: Eeklu
            { "eeklu", new Dictionary<string, uint>(){
                { "belt", 4087 },
                { "belt2", 75355 },
                { "buccaneer", 24983 },
                { "captain", 27609 },
                { "corsair", 25007 },
                { "duster", 24981 },
                { "grimbuccaneer", 27873 },
                { "grimduster", 27895 },
                { "grimjacket", 45979 },
                { "jacket", 24950 },
                { "maskbelt", 72895 },
                { "pinkbuccaneer", 63269 },
                { "pinkcaptain", 28187 },
                { "pinkjacket", 27903 },
                { "zombie", 75432 },
                { "zombiebelt", 72760 }
            }},
            
            // Type: Ekose
            { "ekose", new Dictionary<string, uint>(){
                { "female", 32219 },
                { "femalespace", 29797 },
                { "maleblue", 6520 },
                { "malegreen", 45195 },
                { "malegreenspace", 46921 },
                { "malered", 1684 },
                { "maleredspace", 29798 },
                { "maleyellow", 45009 }
            }},

            // Type: Elemental (Storyteller Only)
            { "elemental", new Dictionary<string, uint>(){
                { "air", 52514 },
                { "earth", 52519 },
                { "fire", 52516 },
                { "life", 52518 },
                { "logic", 52517 },
                { "soulfrost", 75622 },
                { "water", 52515 }
            }},

            // Type: Falkrin
            { "falkrin", new Dictionary<string, uint>(){
                { "bluewarrior", 5205 },
                { "goldwitch", 8861 },
                { "matriarch", 24007 },
                { "purplematriarch", 73307 },
                { "warrior", 8396 },
                { "warrior2", 17359 },
                { "warrior3", 19603 },
                { "witch", 20656 },
                //ARCHIVE UPDATE creatures- used for the mounts
                { "redM", 77078 },
                { "redwarrior", 77079 },
                { "purpleM", 77080 },
                { "purplewarrior", 77081 },
                { "goldM", 77082 },
                { "goldwarrior", 77083 },
                { "greyM", 77084 },
                { "greywarrior", 77085 },
                { "brownM", 77086 },
                { "brownwarrior", 77087 },
                { "goldenF", 77088 },
                { "goldenwitch", 77089 },
                { "whiteF", 77090 },
                { "whitewitch", 77091 },
                { "purpleF", 77092 },
                { "purplewitch", 77093 },
                { "greyF", 77094 },
                { "greywitch", 77095 },
                { "brownF", 77096 },
                { "brownwitch", 77097 }
            }},

            // Type: Freebot
            { "freebot", new Dictionary<string, uint>(){
                { "blue", 26449 },
                { "blueneedles", 24382 },
                { "bronze", 75865 },
                { "bronzeclubs", 24381 },
                { "dominion", 17873 },
                { "exile", 24099 },
                { "green", 14271 },
                { "heavyblue", 52112 },
                { "heavygold", 59184 },
                { "heavygrey", 61852 },
                { "heavyred", 61854 },
                { "pellprobe", 49366 },
                { "reddrills", 70341 },
                { "silverclubs", 19778 }
            }},

            // Type: Garr
            { "garr", new Dictionary<string, uint>(){
                { "green", 69520 },
                { "olive", 69522 },
                { "patrol", 69519 },
                { "red", 69518 },
                { "strain", 69523 }
            }},

            // Type: Girrok
            { "girrok", new Dictionary<string, uint>(){
                { "augmented", 19741 },
                { "black", 1753 },
                { "bone", 23775 },
                { "brown", 17646 },
                { "purple", 19969 },
                { "purplestripe", 45505 },
                { "scarred", 26846 },
                { "skeledroid", 26201 },
                { "strain", 38289 },
                { "white", 19742 },
                { "riding", 73222 },
                { "polar", 73225 },
                { "nightmare", 75208 },
                { "loftite", 75568 },
                //Added by Archive Update *******************
                { "ridingblack", 77018 },
                { "ridingwhite", 77019 },
                { "ridingpurple", 77020 },
                { "ridingscarred", 77021 },
                { "ridingpurplestripe", 77022 },
            }},
            
            // Type: Grumpel
            { "grumpel", new Dictionary<string, uint>(){
                { "base", 17381 },
                { "space", 75253 }
            }},

            // Type: Grund
            { "grund", new Dictionary<string, uint>(){
                { "augmented", 48649 },
                { "grim", 63556 },
                { "grim3", 11902 },
                { "grim6", 70196 },
                { "red", 9634 },
                { "red2", 23588 },
                { "red3", 24951 },
                { "red4", 24986 },
                { "red5", 53395 },
                { "red6", 61901 },
                { "white", 18624 },
                { "white2", 68841 },
                { "white3", 8982 },
                { "white5", 63252 },
                { "zombie", 68632 },
                { "zombie2", 68633 }
            }},

            // Type: Heynar
            { "heynar", new Dictionary<string, uint>(){
                { "grey", 48028 },
                { "icewarhound", 73291 },
                { "purple", 18610 },
                { "warhound", 30377 }
            }},

            // Type: High Priest (Storyteller Only)
            { "highpriest", new Dictionary<string, uint>(){
                { "armored", 48554 },
                { "armored2", 70445 },
                { "armoredwhite", 70893 },
                { "base", 42948 },
                { "dark", 75509 },
                { "strain", 55015 }
            }},

            // Type: Ikthian
            { "ikthian", new Dictionary<string, uint>(){
                { "armor1", 21439 },
                { "armor2", 27765 },
                { "armor3", 27769 },
                { "armor4", 28508 },
                { "base", 21436 },
                { "base2", 26034 },
                { "claws", 21373 }
            }},

            // Type: Jabbit
            { "jabbit", new Dictionary<string, uint>(){
                { "blue", 69412 },
                { "brown", 69413 },
                { "grey", 69414 },
                { "strain", 69416 }
            }},

            // Type: Krogg
            { "krogg", new Dictionary<string, uint>(){
                { "base", 19729 },
                { "highwayman", 23804 }
            }},

            // Type: Kurg
            { "kurg", new Dictionary<string, uint>(){
                { "caravantan", 41810 },
                { "caravanwhite", 24091 },
                { "tan", 42293 },
                { "white", 73288 }
            }},

            // Type: Lopp
            { "lopp", new Dictionary<string, uint>(){
                { "femaleblue", 24142 },
                { "flower", 20810 },
                { "femalegreen", 25285 },
                { "femalegreenspace", 29906 },
                { "femalered", 24353 },
                { "femaleredspace", 27915 },
                { "malegreen", 24116 },
                { "malegreenspace", 28348 },
                { "malered", 25283 },
                { "maleredspace", 28346 },
                { "maleyellow", 24118 },
                { "maleyellowspace", 28347 },
                { "marshal", 20809 },
                { "snowfemale", 24361 },
                { "snowmale", 11010 }
            }},

            // Type: Malverine
            { "malverine", new Dictionary<string, uint>(){
                { "augmented", 32213 },
                { "black", 41659 },
                { "golden", 25982 },
                { "purple", 23851 },
                { "strain", 38071 },
                { "white", 31755 }
            }},

            // Type: Moodie
            { "moodie", new Dictionary<string, uint>(){
                { "chieftain", 69595 },
                { "fighter", 69593 },
                { "slasher", 69594 },
                { "witchdoctor", 69592 }
            }},

            // Type: Nerid
            { "nerid", new Dictionary<string, uint>(){
                { "blue", 30602 },
                { "blue2", 26044 }
            }},

            // Type: Oghra
            { "oghra", new Dictionary<string, uint>(){
                { "augmented", 65441 },
                { "captain", 28079 },
                { "coat", 4091 },
                { "duster", 33426 },
                { "grimduster", 27875 },
                { "grimvest", 63557 },
                { "skin", 9633 },
                { "skirt", 4089 },
                { "vest", 15756 },
                { "zombieskin", 69110 },
                { "zombievest", 75681 }
            }},

            // Type: Osun (Storyteller Only)
            { "osun", new Dictionary<string, uint>(){
                { "warlord", 13021 },
                { "icewarlord", 75614 },
                { "icewarlord2", 75459 },
                { "redwarlord", 70444 },
                { "warrior", 13019 },
                { "icewarrior", 75615 },
                { "redwarrior", 73099 },
                { "warrior2", 14342 },
                { "icewarrior2", 71367 },
                { "warrior3", 14343 },
                { "redwarrior3", 48177 },
                { "strainwarrior", 55016 },
                { "witch", 15202 },
                { "witch2", 71767 },
                { "blueghostwitch", 75621 },
                { "redghostwitch", 75617 },
                { "redwitch2", 48295 },
                { "icewitch", 71250 },
                { "icewitch2", 70373 },
                { "strainwitch", 52969 }
            }},

            // Type: Pell
            { "pell", new Dictionary<string, uint>(){
                { "augmented", 8257 },
                { "brown", 14397 },
                { "brown2", 21615 },
                { "brown3", 43195 },
                { "brown4", 41703 },
                { "brownarmored", 70873 },
                { "brownarmored2", 20706 },
                { "brownarmored3", 20708 },
                { "greybase", 30464 },
                { "grey", 26603 },
                { "grey2", 30507 },
                { "grey3", 75458 },
                { "greyarmored", 30450 },
                { "greyarmored2", 30462 },
                { "greyarmored3", 49352 },
                { "greyarmored4", 49346 },
                //Added by Archive Update ********************
                { "white", 37326 },
                { "white2", 37351 },
                { "white3", 35914 },
                { "white4", 35916 },
                { "whitearmored", 35912 },
                { "whitearmored2", 37369 },
                { "whitearmored3", 38676 },
                { "whitearmored4", 42911 },
                { "white1a", 77023 },
                { "white2a", 77024 },
                { "white3a", 77025 },
                { "whitearmored1a", 77026 },
                { "whitearmored2a", 77027 },
                { "whitearmored3a", 77028 },
                { "whitearmored4a", 77029 },
                { "white1b", 77030 },
                { "white2b", 77031 },
                { "white3b", 77032 },
                { "whitearmored1b", 77033 },
                { "whitearmored2b", 77034 },
                { "whitearmored3b", 77035 },
                { "whitearmored4b", 77036 },
                { "brown1a", 77037 },
                { "brown2a", 77038 },
                { "brown3a", 77039 },
                { "brown4a", 77040 },
                { "brownarmored1a", 77041 },
                { "brownarmored2a", 77042 },
                { "brownarmored3a", 77043 },
                { "brownarmored4a", 77044 },
                { "brown1b", 77045 },
                { "brown2b", 77046 },
                { "brown3b", 77047 },
                { "brown4b", 77048 },
                { "brownarmored1b", 77049 },
                { "brownarmored2b", 77050 },
                { "brownarmored3b", 77051 },
                { "brownarmored4b", 77052 },
                { "grey1a", 77053 },
                { "grey2a", 77054 },
                { "grey3a", 77055 },
                { "grey4a", 77056 },
                { "greyarmored1a", 77057 },
                { "greyarmored2a", 77058 },
                { "greyarmored3a", 77059 },
                { "greyarmored4a", 77060 },
                { "grey1b", 77061 },
                { "grey2b", 77062 },
                { "grey3b", 77063 },
                { "grey4b", 77064 },
                { "greyarmored1b", 77065 },
                { "greyarmored2b", 77066 },
                { "greyarmored3b", 77067 },
                { "greyarmored4b", 77068 },
            }},

            // Type: Protostar
            { "protostar", new Dictionary<string, uint>(){
                { "employee", 7301 },
                { "phineas", 26454 },
                { "phineas2", 65788 },
                { "papaphineas", 72565 },
                { "modificator", 51913 }
            }},

            // Type: Pumera
            { "pumera", new Dictionary<string, uint>(){
                { "chua", 69430 },
                { "frosted", 69421 },
                { "golden", 69424 },
                { "grey", 69417 },
                { "magenta", 69423 },
                { "maroon", 69420 },
                { "sabertooth", 69427 },
                { "sienna", 69422 },
                { "snowy", 69418 },
                { "snowstripe", 69426 },
                { "steely", 69428 },
                { "strain", 69432 },
                { "tawny", 69419 },
                { "torine", 69429 },
                { "riding", 73213 },
                { "pridelord", 73717 },
                { "redpridelord", 75569 },
                { "hardlight", 75340 },
                { "frostbite", 75648 },
                { "exotic", 75872 },
                { "whitevale", 69425 },
                { "mechari", 73411 },
                //From archive update **************************
                { "ridingtawny", 76999 },
                { "ridingsienna", 77000 },
                { "ridingmagenta", 77001 },
                { "ridinggolden", 77002 },
                { "ridingmaroon", 77003 },
                { "ridingsnowy", 77004 },
                { "ridingsnowstripe", 77005 },
                { "ridingsteely", 77006 },
                { "ridingwhitevale", 77007 },
            }},

            // Type: Ravenok
            { "ravenok", new Dictionary<string, uint>(){
                { "purple", 69449 },
                { "steelblue", 69450 },
                { "teal", 69453 },
                { "golden", 69454 },
                { "augmented", 69455 },
                { "snowy", 69456 },
                { "strain", 69457 }
            }},

            // Type: Roan
            { "roan", new Dictionary<string, uint>(){
                { "brownbull", 1741 },
                { "browncow", 2065 },
                { "greybull", 15640 }
            }},

            // Type: Rowsdower
            { "rowsdower", new Dictionary<string, uint>(){
                { "white", 12921 },
                { "demonic", 48437 },
                { "augmented", 69474 },
                { "pink", 69475 },
                { "party", 70316 },
            }},

            // Type: Slank
            { "slank", new Dictionary<string, uint>(){
                { "", 27250 },
            }},

            // Type: Strain (Storyteller Only)
            { "strain", new Dictionary<string, uint>(){
                { "brute", 55050 },
                { "technobrute", 52970 },
                { "corruptor", 30208 },
                { "technocorruptor", 52963 },
                { "crawler", 48146 },
                { "mauler", 55010 },
                { "technomauler", 52964 },
                { "peep", 37962 },
                { "ravager", 30210 },
                { "technoravager", 52968 }
            }},

            // Type: Tank (Storyteller Only)
            { "tank", new Dictionary<string, uint>(){
                { "dominion", 47567 },
                { "exile", 41168 }
            }},

            // Type: Triton (Storyteller Only)
            { "triton", new Dictionary<string, uint>(){
                { "armored", 34094 },
                { "armored2", 48592 },
                { "armored3", 61635 },
                { "base", 34093 },
                { "strain", 55018 },
            }},

            // Type: Vind
            { "vind", new Dictionary<string, uint>(){
                { "", 2410 },
            }},

            // Type: Warbot (Storyteller Only)
            { "warbot", new Dictionary<string, uint>(){
                { "dominion", 20998 },
                { "exile", 21544 },
                { "osun", 32519 },
                { "ikthian", 34644 }
            }},

            // Type: Witch Giant (Storyteller Only)
            { "witchgiant", new Dictionary<string, uint>(){
                { "life", 21804 },
                { "ice", 20857 },
                { "strain", 49391 },
            }},

            // Type: Equivar
            { "equivar", new Dictionary<string, uint>(){
                { "brown", 8725 },
                { "luminous", 61693 },
                { "purple", 71475 },
                { "blue", 71476 },
                { "green", 71477 },
                { "black", 71481 },
                { "floral", 72621 },
                { "augmented", 73269 },
                { "verdant", 74855 },
                { "ice", 75683 },
                { "technophage", 75907 },
            }},

            // Type: Equirin ************* (mostly) Added by Archive Update
            { "equirin", new Dictionary<string, uint>(){
                { "brown", 70656 }, // this one already existed before archive update
                { "luminous", 77013 },
                { "purple", 77011 },
                { "blue", 77008 },
                { "green", 77010 },
                { "black", 77012 },
                { "verdant", 77009 },
            }},

            // Type: Velocirex
            { "velocirex", new Dictionary<string, uint>(){
                { "green", 54261 },
                { "alien", 61390 },
                { "badlands", 71419 },
                { "ascendant", 71464 },
                { "contagion", 75109 },
                { "skeletal", 75204 },
                { "electric", 75205 },
                { "ebon", 75906 },
                { "mecha", 75844 },
                { "ice", 71038 },
            }},

            // Type: Trask
            { "trask", new Dictionary<string, uint>(){
                { "purple", 5212 },
                { "dreg", 61762 },
                { "ice", 71429 },
                { "jungle", 71484 },
                { "toxic", 71486 },
                { "blurple", 71485 },
                { "pinkbelly", 71487 },
                { "strain", 73555 },
                { "darkspur", 75293 },
                { "hardlight", 75366 },
                //Added by Archive Update ********************
                { "blue", 77014 },
                { "red", 77015 },
                { "yellow", 77016 },
                { "black", 77017 },
            }},

            // Type: Warpig
            { "warpig", new Dictionary<string, uint>(){
                { "red", 22297 },
                { "redbase", 9622 },
                { "savage", 61428 },
                { "reptile", 71483 },
                { "skeletal", 72355 },
                { "armoredred", 73076 },
                //Added by Archive Update ********************
                { "direred", 77113 },
                { "diregrey", 77114 },
                { "direbrown", 77115 },
                { "direpurple", 77116 },
                { "ice", 73305 }
            }},

            // Type: Woolie
            { "woolie", new Dictionary<string, uint>(){
                { "purple", 56827 },
                { "war", 61823 },
                { "green", 71478 },
                { "yellow", 71479 },
                { "blue", 71480 },
                { "bandit", 71482 },
                { "empyrean", 75107 },
                { "dream", 75218 }
            }},

            // Type: Uniblade
            { "uniblade", new Dictionary<string, uint>(){
                { "basic", 48178 },
                { "retro", 61392 },
                { "hotrod", 61393 }
            }},

            // Type: Grinder
            { "grinder", new Dictionary<string, uint>(){
                { "basic", 11085 },
                { "rally", 61833 }
            }},

            // Type: Orbitron
            { "orbitron", new Dictionary<string, uint>(){
                { "basic", 52319 },
                { "marauder", 61391 }
            }},

            // Type: Speeder
            { "speeder", new Dictionary<string, uint>(){
                { "krogg", 75649 },
                { "goldkrogg", 75724 },
                { "blackkrogg", 75725 },
                { "whitekrogg", 75726 },
                { "osun", 71531 },
                { "arctic", 73037 }
            }},

            // Type: Hellhound
            { "hellhound", new Dictionary<string, uint>(){
                { "red", 75743 },
                { "blue", 75744 },
                { "green", 75745 },
                { "gold", 75746 }
            }},

            // Type: Mech
            { "mech", new Dictionary<string, uint>(){
                { "red", 75918 },
                { "blue", 75919 },
                { "gold", 75920 },
                { "yellow", 75921 }
            }},

            // Type: Spiderbot
            { "spiderbot", new Dictionary<string, uint>(){
                { "protostar", 70020 },
                { "goldprotostar", 73342 },
                { "phage", 75590 },
                { "crystal", 75905 },
                { "lava", 75917 }
            }},

            // Type: Skeledroid
            { "skeledroid", new Dictionary<string, uint>(){
                { "", 26193 }
            }},

            // Type: Gorganoth (Storyteller Only)
            { "gorganoth", new Dictionary<string, uint>(){
                { "skeledroid", 48065 },
                { "brown", 1743 },
                { "augmented", 22539 },
                { "dreg", 24244 },
                { "undead", 28077 },
                { "squirg", 62010 },
                { "white", 63122 }
            }},

            // Type: Invisible
            { "invisible", new Dictionary<string, uint>(){
                { "", 5609 },
            }},

            // Type: Scrab
            { "scrab", new Dictionary<string, uint>(){
                { "dune", 69566 },
                { "crimson", 69567 },
                { "dreg", 69568 },
                { "strain", 69571 },
                { "silvershell", 73287 }
            }},

            // Type: Buzzbing
            { "buzzbing", new Dictionary<string, uint>(){
                { "honey", 69530 },
                { "honeyblue", 69531 },
                { "augmented", 69532 },
                { "grey", 69534 },
                { "purple", 69535 },
                { "alien", 69536 },
                { "goldjacket", 69537 },
                { "strain", 69539 },
                { "chua", 69540 },
                { "goldshell", 69541 },
                { "silvershell", 69542 },
                { "bumble", 69538 }
            }},

            // Type: Spider
            { "spider", new Dictionary<string, uint>(){
                { "gold", 69609 },
                { "blackred", 69610 },
                { "blackgreen", 75520 },
                { "augmented", 69611 },
                { "strain", 69612 },
                { "silvershell", 69613 }
            }},

            // Type: Spikehorde
            { "spikehorde", new Dictionary<string, uint>(){
                { "indigo", 69499 },
                { "mako", 69500 },
                { "rock", 69501 },
                { "emerald", 69502 },
                { "black", 69503 },
                { "hunter", 69504 },
                { "maroon", 69505 }
            }},

            // Type: Scanbot
            { "scanbot", new Dictionary<string, uint>(){
                { "basic", 32151 },
                { "wings", 53929 }
            }},

            // Type: Stemdragon (Storyteller Only)
            { "stemdragon", new Dictionary<string, uint>(){
                { "green", 2016 },
                { "augmented", 19847 },
                { "strain", 20115 },
                { "red", 35987 },
                { "logicleaf", 66270 },
                { "primeval", 75887 }
            }},

            // Type: NPC Human
            { "npchuman", new Dictionary<string, uint>(){
                { "burly", 13386 },
                { "old", 16636 }
            }},

            // Type: Gribbon
            { "gribbon", new Dictionary<string, uint>(){
                { "grey", 69553 },
                { "rainbow", 69554 }
            }},

            // Type: Vulcarrion
            { "vulcarrion", new Dictionary<string, uint>(){
                { "falkrin", 68801 },
                { "grey", 69543 },
                { "brown", 69544 },
                { "augmented", 69546 },
                { "armoredfalkrin", 69548 },
                { "phoenix", 69549 }
            }},

            // Type: Hookfoot
            { "hookfoot", new Dictionary<string, uint>(){
                { "moonwalker", 69601 },
                { "darkfeeder", 69602 },
                { "augmented", 69603 }
            }},

            // Type: Splorg
            { "splorg", new Dictionary<string, uint>(){
                { "blue", 69577 },
                { "violet", 69578 },
                { "teal", 69579 },
                { "dynamite", 69580 }
            }},

            // Type: Skug
            { "skug", new Dictionary<string, uint>(){
                { "larva", 69581 },
                { "violet", 69516 },
                { "pink", 69517 }
            }},

            // Type: Squirg
            { "squirg", new Dictionary<string, uint>(){
                { "pink", 69582 },
                { "blue", 69583 },
                { "peach", 69584 },
                { "strain", 69585 }
            }},

            // Type: Jellibolli
            { "jellibolli", new Dictionary<string, uint>(){
                { "lavender", 69555 },
                { "polyp", 69556 },
                { "sunfire", 69557 }
            }},

            // Type: Steamglider
            { "steamglider", new Dictionary<string, uint>(){
                { "pinkfin", 69558 },
                { "greenfin", 69559 },
                { "strain", 69560 },
                { "yellowfin", 69561 },
                { "tealfin", 69562 },
                { "sunset", 69563 },
                { "sunrise", 69564 },
                { "moonglow", 69565 }
            }},

            // Type: rootbrute
            { "rootbrute", new Dictionary<string, uint>(){
                { "browncap", 69604 },
                { "snowcap", 69605 },
                { "firecap", 69606 },
                { "augmented", 69607 },
                { "strain", 69608 }
            }},

            // Type: Terminite
            { "terminite", new Dictionary<string, uint>(){
                { "white", 69616 },
                { "brown", 69617 },
                { "grey", 69618 },
                { "alien", 69619 }
            }}
        };
        
        private static readonly Dictionary<string, uint> CreatureLibrary = new Dictionary<string, uint>() {
            // Type: Boss (Storyteller Only)
            { "boss", 60057 },
            
            // Type: Canimid
            { "canimid", 19299 },
            
            // Type: Chompy
            { "chompy", 69490 },
            
            // Type: Construct (Storyteller Only)
            { "construct", 45865 },
            
            // Type: Dagun
            { "dagun", 69407 },
            
            // Type: Dawngrazer
            { "dawngrazer", 69404 },
            
            // Type: Eeklu
            { "eeklu", 4087 },
            
            // Type: Ekose
            { "ekose", 32219 },

            // Type: Elemental (Storyteller Only)
            { "elemental", 52519 },

            // Type: Falkrin
            { "falkrin", 20648 },

            // Type: Freebot
            { "freebot", 26449 },

            // Type: Garr
            { "garr", 69520 },

            // Type: Girrok
            { "girrok", 1753 },
            
            // Type: Grumpel
            { "grumpel", 17381 },

            // Type: Grund
            { "grund", 9634 },

            // Type: Heynar
            { "heynar", 48028 },

            // Type: High Priest (Storyteller Only)
            { "highpriest", 42948 },

            // Type: Ikthian
            { "ikthian", 21436 },

            // Type: Jabbit
            { "jabbit", 69412 },

            // Type: Krogg
            { "krogg", 19729 },

            // Type: Kurg
            { "kurg", 42293 },

            // Type: Lopp
            { "lopp", 20810 },

            // Type: Malverine
            { "malverine", 41659 },

            // Type: Moodie
            { "moodie", 69593 },

            // Type: Nerid
            { "nerid", 30602 },

            // Type: Oghra
            { "oghra", 28079 },

            // Type: Osun (Storyteller Only)
            { "osun", 13019 },

            // Type: Pell
            { "pell", 14397 },

            // Type: Protostar
            { "protostar", 7301 },

            // Type: Pumera
            { "pumera", 69422 },

            // Type: Ravenok
            { "ravenok", 69453 },

            // Type: Roan
            { "roan", 1741 },

            // Type: Rowsdower
            { "rowsdower", 12921 },

            // Type: Slank
            { "slank", 27250 },

            // Type: Strain (Storyteller Only)
            { "strain", 48146 },

            // Type: Tank (Storyteller Only)
            { "tank", 47567 },

            // Type: Triton (Storyteller Only)
            { "triton", 34093 },

            // Type: Vind
            { "vind", 2410 },

            // Type: Warbot (Storyteller Only)
            { "warbot", 20998 },

            // Type: Witch Giant (Storyteller Only)
            { "witchgiant", 21804 },

            // Type: Equivar
            { "equivar", 8725 },

            // Type: Equirin
            { "equirin", 70656 },

            // Type: Velocirex
            { "velocirex", 54261 },

            // Type: Trask
            { "trask", 5212 },

            // Type: Warpig
            { "warpig", 22297 },

            // Type: Woolie
            { "woolie", 56827 },

            // Type: Uniblade
            { "uniblade", 48178 },

            // Type: Grinder
            { "grinder", 11085 },

            // Type: Orbitron
            { "orbitron", 52319 },

            // Type: Speeder
            { "speeder", 75649 },

            // Type: Hellhound
            { "hellhound", 75743 },

            // Type: Mech
            { "mech", 75918 },

            // Type: Spiderbot
            { "spiderbot", 70020 },

            // Type: Skeledroid
            { "skeledroid", 26193 },

            // Type: Gorganoth (Storyteller Only)
            { "gorganoth", 48065 },

            // Type: Invisible
            { "invisible", 5609 },

            // Type: Scrab
            { "scrab", 69566 },

            // Type: Buzzbing
            { "buzzbing", 69530 },

            // Type: Spider
            { "spider", 69610 },

            // Type: Spikehorde
            { "spikehorde", 69499 },

            // Type: Scanbot
            { "scanbot", 32151 },

            // Type: Stemdragon (Storyteller Only)
            { "stemdragon", 2016 },

            // Type: NPC Human
            { "npchuman", 13386 },

            // Type: Gribbon
            { "gribbon", 69553 },

            // Type: Vulcarrion
            { "vulcarrion", 69543 },

            // Type: Hookfoot
            { "hookfoot", 69601 },

            // Type: Splorg
            { "splorg", 69577 },

            // Type: Skug
            { "skug", 69516 },

            // Type: Squirg
            { "squirg", 69582 },

            // Type: Jellibolli
            { "jellibolli", 69555 },

            // Type: Steamglider
            { "steamglider", 69558 },

            // Type: rootbrute
            { "rootbrute", 69604 },

            // Type: Terminite
            { "terminite", 69616} 
        };

        /// <summary>
        /// (GENESIS PRIME) Creature types privileged to the Storyteller Role.
        /// </summary>
        private static readonly List<string> StorytellerOnly = new List<string>() {
            "boss",
            "construct",
            "elemental",
            "highpriest",
            "osun",
            "strain",
            "tank",
            "triton",
            "warbot",
            "witchgiant",
            "gorganoth",
            "stemdragon"
        };

        /// <summary>
        /// (GENESIS PRIME) Get the ID of a legal creature type/variant combination for morph/summon commands.
        /// Returns a nullable uint. Null means the creature type could not be found.
        /// </summary>
        public static uint? GetCreatureIdFromType(string creatureType, string creatureVariant)
        {
            if(string.IsNullOrWhiteSpace(creatureType))
            {
                return null;
            }
            Dictionary<string, uint> creatureSubLibrary;
            // get the creature type-specific dictionary

            if(string.IsNullOrWhiteSpace(creatureVariant))
            {
                if(CreatureLibrary.TryGetValue(creatureType.ToLower(), out uint creatureID))
                {
                    return creatureID;
                }
                return null;
            }

            //log.Info($"Looking up {creatureType} in the Creature Library...");
            if (CreatureVariantLibrary.TryGetValue(creatureType.ToLower(), out creatureSubLibrary))
            {
                //log.Info($"{creatureType} found as a creature type!");
                // get the creature ID corresponding to the variant
                if (creatureSubLibrary.TryGetValue(creatureVariant.ToLower(), out uint returnCreatureId))
                {
                    return returnCreatureId;
                }
            }
            return null;
        }

        /// <summary>
        /// (GENESIS PRIME) Get a list of valid creature types.
        /// </summary>
        public static List<string> getCreatureTypeList(bool storyteller)
        {
            if (storyteller)
            {
                return CreatureVariantLibrary.Keys.ToList();
            }
            // not a storyteller, then.
            return CreatureVariantLibrary.Keys.Where(k => !IsStoryTellerOnly(k)).ToList();
        }

        /// <summary>
        /// (GENESIS PRIME) Get a list of valid creature variants for a creature type.
        /// </summary>
        public static List<string> getCreatureVariantsForType(string creatureType)
        {
            Dictionary<string, uint> creatureSubLibrary;
            // get the creature type-specific dictionary
            //log.Info($"Looking up {creatureType} in the Creature Library...");
            if (CreatureVariantLibrary.TryGetValue(creatureType, out creatureSubLibrary))
            {
                return new List<string>(creatureSubLibrary.Keys);
            }
            return null;
        }

        public static bool IsStoryTellerOnly(string creatureType)
        {
            return StorytellerOnly.Contains(creatureType);
        }
    }
}
