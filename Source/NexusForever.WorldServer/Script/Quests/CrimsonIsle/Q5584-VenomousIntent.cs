using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;

namespace NexusForever.WorldServer.Script.Quests.CrimsonIsle
{
    [Script(24215)]
    public class Q5584_VenomousIntent_TrappedAssistant : CreatureScript
    {
        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            me.StandState = StandState.State2;
            me.ModifyHealth(-me.MaxHealth);
        }
    }
}
