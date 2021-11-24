using NexusForever.Shared.Game.Events;
using NexusForever.WorldServer.Game.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Script.Creature.NorthernWilds
{
    [Script(12653)]
    public class DominionGate : CreatureScript
    {
        const uint DOOR_ICEFURY_GATE = 16799;

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            Door doorUnit = me.GetVisibleCreature<Door>(DOOR_ICEFURY_GATE).FirstOrDefault();
            if (doorUnit != null)
            {
                if (doorUnit.IsOpen)
                    return;
                
                doorUnit.OpenDoor();
                doorUnit.QueueEvent(new DelayEvent(TimeSpan.FromSeconds(10d), () => 
                {
                    doorUnit.CloseDoor();
                }));
            }

        }
    }
}
