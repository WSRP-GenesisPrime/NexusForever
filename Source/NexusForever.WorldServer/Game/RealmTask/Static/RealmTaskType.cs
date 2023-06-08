using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Game.RealmTask.Static
{
    public enum RealmTaskType
    {
        CharacterRename                 = 0,
        AccountCharacterTransfer        = 1,
        GuildRename                     = 2,
        CharacterInventoryClear         = 3,
        CharacterClassChange            = 4,
        CharacterNeighborRequest        = 5,
        CharacterGuildInvite            = 6
    }
}
