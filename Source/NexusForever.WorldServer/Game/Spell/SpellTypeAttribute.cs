using NexusForever.WorldServer.Game.Spell.Static;
using System;

namespace NexusForever.WorldServer.Game.Spell
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SpellTypeAttribute : Attribute
    {
        public CastMethod CastMethod { get; }

        public SpellTypeAttribute(CastMethod castMethod)
        {
            CastMethod = castMethod;
        }
    }
}
