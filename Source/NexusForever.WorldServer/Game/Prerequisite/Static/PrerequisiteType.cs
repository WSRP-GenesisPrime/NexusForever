﻿namespace NexusForever.WorldServer.Game.Prerequisite.Static
{
    // TODO: name these from PrerequisiteType.tbl error messages
    public enum PrerequisiteType
    {
        None            = 0,
        Level           = 1,
        Race            = 2,
        Class           = 3,
        Faction         = 4,
        Reputation      = 5,
        Quest           = 6,
        Achievement     = 7,
        Gender          = 10,
        Prerequisite    = 11,
        HasBuff         = 15,
        Zone            = 26,
        Path            = 52,
        Vital           = 73,
        Disguise        = 105,
        SpellObj        = 129,
		    /// <summary>
        /// Checks for an ObjectId, which is a hashed petflair id.
        /// </summary>
        HoverboardFlair = 190,
        /// <summary>
        /// Used for Mount checks
        /// </summary>
        GroundMountArea = 194,
        /// <summary>
        /// Used for Mount checks
        /// </summary>
        HoverboardArea  = 195,
        SpellBaseId     = 214,
        Plane           = 232,
        BaseFaction     = 250,
        Unhealthy       = 269,
        Loyalty         = 270
    }
}
