using NexusForever.WorldServer.Game.Cinematic.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Cinematic
{
    public class ActorVisibility : IKeyframeAction
    {
        public uint Delay { get; }
        public uint ActorUnitId { get; }
        public bool Hide { get; }
        public bool Unknown0 { get; }

        public ActorVisibility(uint delay, Actor actor, bool hide = false)
        {
            Delay = delay;
            ActorUnitId = actor.Id;
            Hide = hide;
        }

        public ActorVisibility(uint delay, uint unitId, bool hide = false)
        {
            Delay = delay;
            ActorUnitId = unitId;
            Hide = hide;
        }

        public void Send(WorldSession session)
        {
            session.EnqueueMessageEncrypted(new ServerCinematicActorVisibility
            {
                Delay = Delay,
                UnitId = ActorUnitId,
                Hide = Hide,
                Unknown0 = Unknown0
            });
        }
    }
}
