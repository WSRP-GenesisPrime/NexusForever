using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Script.Creature
{
    [Script(16718)] // Marauder Mine - Algoroc
    [Script(24251)] // Exile Mine - Crimson Isle
    public class MarauderMine : CreatureScript
    {
        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            (me as UnitEntity).CastSpell(26443, new SpellParameters
            {
                PrimaryTargetId = activator.Guid,
                CompleteAction = (SpellParameters parameters) =>
                {
                    (me as UnitEntity).ModifyHealth(-me.Health);
                }
            });
        }
    }
}
