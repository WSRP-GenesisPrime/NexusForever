using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell;

namespace NexusForever.WorldServer.Script.Creature
{
    [Script(27768)] // Bramble Trap
    public class BrambleTrap : CreatureScript
    {
        const uint SPELL_PENALTY = 46051;

        public override void OnCreate(WorldEntity me)
        {
            base.OnCreate(me);
        }

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            base.OnActivateSuccess(me, activator);

            if ((activator as UnitEntity).HasSpell(s => s.Spell4Id == SPELL_PENALTY, out Spell spell))
                spell.Finish();
            me.ModifyHealth(-me.MaxHealth);
        }

        public override void OnActivateFail(WorldEntity me, WorldEntity activator)
        {
            base.OnActivateFail(me, activator);

            (activator as UnitEntity).CastSpell(SPELL_PENALTY, new SpellParameters
            {
                UserInitiatedSpellCast = false
            });
        }
    }
}
