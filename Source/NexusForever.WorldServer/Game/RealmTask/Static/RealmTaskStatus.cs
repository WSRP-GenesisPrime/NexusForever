using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.RealmTask.Static
{
    public enum RealmTaskStatus
    {
        Staged          = 0,
        Completed       = 1,
        Failed          = 2,
        Retry           = 3
    }
}
