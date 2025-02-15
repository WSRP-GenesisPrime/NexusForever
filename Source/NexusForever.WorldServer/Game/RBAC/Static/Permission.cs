﻿namespace NexusForever.WorldServer.Game.RBAC.Static
{
    /// <remarks>
    /// Values must be explicitly numbered!
    /// This is to prevent new entries shifting the ids of old ones.
    /// </remarks>
    /// <remarks>
    /// IDs to reuse:
    /// 
    /// </remarks>
    public enum Permission
    {
        None,

        // help
        Help                        = 4,

        // account
        Account                     = 1,
        AccountCreate               = 2,
        AccountDelete               = 3,

        // rbac
        RBAC                        = 7,
        RBACAccount                 = 8,
        RBACAccountPermission       = 9,
        RBACAccountPermissionGrant  = 10,
        RBACAccountPermissionRevoke = 11,
        RBACAccountRole             = 12,
        RBACAccountRoleGrant        = 13,
        RBACAccountRoleRevoke       = 14,

        // achievement
        Achievement                 = 15,
        AchievementGrant            = 16,
        AchievementUpdate           = 17,

        // broadcast
        Broadcast                   = 18,
        BroadcastMessage            = 19,

        // character
        Character                   = 20,
        CharacterXP                 = 21,
        CharacterLevel              = 22,
        CharacterSave               = 5,

        // currency
        Currency                    = 23,
        CurrencyAccount             = 24,
        CurrencyAccountAdd          = 25,
        CurrencyAccountList         = 26,
        CurrencyCharacter           = 27,
        CurrencyCharacterAdd        = 28,
        CurrencyCharacterList       = 29,

        // disable
        Disable                     = 30,
        DisableInfo                 = 31,
        DisableReload               = 32,

        // door
        Door                        = 33,
        DoorOpen                    = 34,
        DoorClose                   = 35,

        // entitlement
        Entitlement                 = 36,
        EntitlementCharacter        = 37,
        EntitlementCharacterAdd     = 38,
        EntitlementCharacterList    = 39,
        EntitlementAccount          = 40,
        EntitlementAccountAdd       = 41,
        EntitlementAccountList      = 42,

        // entity
        Entity                      = 43,
        EntityInfo                  = 44,
        EntityProperties            = 45,
        EntityModify                = 60,
        EntityModifyDisplayInfo     = 61,

        // generic unlock
        Generic                     = 46,
        GenericUnlock               = 47,
        GenericUnlockAll            = 48,
        GenericList                 = 49,

        // teleport
        Teleport                    = 50,
        TeleportCoordinates         = 51,
        TeleportLocation            = 52,
        TeleportName                = 53,

        // housing
        House                       = 54,
        HouseDecor                  = 55,
        HouseDecorAdd               = 56,
        HouseDecorLookup            = 57,
        HouseTeleport               = 58,

        // item
        Item                        = 59,
        ItemAdd                     = 6,
        ItemLookup                  = 92,

        // movement
        Movement                    = 62,
        MovementSpline              = 63,
        MovementSplineAdd           = 64,
        MovementSplineClear         = 65,
        MovementSplineLaunch        = 66,
        MovementGenerator           = 67,
        MovementGeneratorDirect     = 68,
        MovementGeneratorRandom     = 69,

        // path
        Path                        = 70,
        PathUnlock                  = 71,
        PathActivate                = 72,
        PathXP                      = 73,

        // pet
        Pet                         = 74,
        PetUnlockFlair              = 75,

        // quest
        Quest                       = 76,
        QuestAdd                    = 77,
        QuestAchieve                = 78,
        QuestAchieveObjective       = 79,
        QuestObjective              = 80,
        QuestKill                   = 81,
        QuestList                   = 110,

        // spell
        Spell                       = 83,
        SpellAdd                    = 84,
        SpellCast                   = 85,
        SpellResetCooldown          = 86,

        // title
        Title                       = 87,
        TitleAdd                    = 88,
        TitleRevoke                 = 89,
        TitleAll                    = 90,
        TitleNone                   = 91,

        // realm
        Realm                       = 82,
        RealmMOTD                   = 94,
        RealmShutdown               = 107,
        RealmShutdownStart          = 108,
        RealmShutdownCancel         = 109,
        RealmMaxPlayers             = 111,

        // story board
        Story                       = 95,
        StoryPanel                  = 96,
        StoryCommunicator           = 97,

        // reputation
        Reputation                  = 98,
        ReputationUpdate            = 99,

        // guild
        Guild                       = 100,
        GuildRegister               = 101,
        GuildJoin                   = 102,

        // map
        Map                         = 103,
        MapUnload                   = 104,
        MapPlayerRemove             = 105,
        MapPlayerRemoveCancel       = 106,

        // Genesis Prime: morph command
        Morph                       = 50000,
        MorphStoryteller            = 50001,

        // Genesis Prime: emote command
        Emote                       = 50100,

        // Genesis Prime: chron command
        Chron                       = 50200,

        // Genesis Prime: xroll command
        XRoll                       = 50300,

        // GP: !character props/resetprops
        CharacterProps              = 50400,

        // GP: boost
        Boost                       = 50500,

        // GP: !house remodel
        HouseRemodel                = 50600,

        // GP: realm
        RealmOnline                 = 50700,
        RealmUptime                 = 50710,

        // GP: Change account password
        AccountChangePass           = 50800,
        AccountChangeMyPass         = 50810,

        // GP: Costume commands
        Costume                     = 60000,
        CostumeOverride             = 60010,
        CostumeOverrideId           = 60020,
        
        // GP: 18+ flag
        Adult                       = 61000,
        AdultPlotLockOwner          = 61100,
        AdultPlotLockNonOwner       = 61150,
        AdultPlotAlert              = 61200,

        // non command permissions
        InstantLogout               = 10000,
        Signature                   = 10001,
        BypassInstanceLimits        = 10002,
        GMFlag                      = 10003
    }
}
