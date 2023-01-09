using NexusForever.WorldServer.Game.Entity;

namespace NexusForever.WorldServer.Script.Creature.City
{
    [Script(26350)]
    public class HousePortal : CreatureScript
    {
        readonly uint[] CONST_SPELL_TRAINING = {
            22919, // Recall - House
            25520  // Escape House
        };
        public override void OnCreate(WorldEntity me)
        {
            base.OnCreate(me);

            me.ActivateSpell4Id = 39111u;
        }

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            base.OnActivateSuccess(me, activator);

            if (!(activator is Player player))
                return;

            foreach (uint spellBaseId in CONST_SPELL_TRAINING)
                if (player.SpellManager.GetSpell(spellBaseId) == null)
                    player.SpellManager.AddSpell(spellBaseId);
        }
    }
}
