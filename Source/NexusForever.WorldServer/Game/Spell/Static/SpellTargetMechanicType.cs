using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Spell.Static
{
    /// <summary>
    /// This appears to be an indicator of the location at which target acquisition begins.
    /// </summary>
    /// <remarks>Unsure exactly how this works at this time. It appears to be an optimisation to allow for quicker, accurate selection. Needs investigation.</remarks>
    public enum SpellTargetMechanicType
    {
        Self            = 0,
        PrimaryTarget   = 1,
        SecondaryTarget = 2
    }
}
