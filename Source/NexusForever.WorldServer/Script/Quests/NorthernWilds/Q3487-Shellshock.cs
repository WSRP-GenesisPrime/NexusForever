using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest.Static;
using System.Numerics;

namespace NexusForever.WorldServer.Script.Quests.NorthernWilds
{
    public class Q3487_Shellshock
    {
        [Script(11251)]
        public class Q3487_Shellshock_DominionCannon : CreatureScript
        {
            public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
            {
                if (activator is not Player player)
                    return;

                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.Unknown10, me.CreatureId, me.QuestChecklistIdx);
            }
        }

        [Script(12526)]
        public class Q3487_Shellshock_Ultrabot : CreatureScript
        {
            private Vector3 LOC_DESTINATION = new Vector3(4450f, -700f, -5150f);

            public override void OnAddToMap(WorldEntity me)
            {
                me.MovementManager.MoveTo(LOC_DESTINATION, 5f);
            }
        }
    }
}
