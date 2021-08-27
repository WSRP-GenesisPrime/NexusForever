using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.Quest.Static
{
    [Flags]
    public enum QuestObjectiveFlags
    {
        None                = 0x0000,
        Optional            = 0x0002,
        Hidden              = 0x0008
    }
}
