﻿using System;

namespace NexusForever.WorldServer.Game.Entity.Static
{
    [Flags]
    public enum RezType
    {
        None                    = 0,
        WakeHere                = 1,
        Holocrypt               = 2,
        SpellCasterLocation     = 4,
        ExitInstance            = 32,
        WakeHereServiceToken    = 64,


        // TODO: Add Holocrypt to below masks when we support them
        OpenWorld               = WakeHere | WakeHereServiceToken,
        Dungeon                 = ExitInstance,
        DungeonInCombat         = ExitInstance
    }
}
