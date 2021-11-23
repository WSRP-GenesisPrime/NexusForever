using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Game.Entity
{
    public class Velocity : Move
    {
        public bool HasStopped()
        {
            return X == 0 && Y == 0 && Z == 0;
        }
    }
}
