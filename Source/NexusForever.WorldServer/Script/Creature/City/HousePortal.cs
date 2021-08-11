using NexusForever.Shared;
using NexusForever.Shared.Game.Events;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Housing;

namespace NexusForever.WorldServer.Script.Creature.City
{
    [Script(26350)]
    public class HousePortal : CreatureScript
    {
        readonly uint[] CONST_SPELL_TRAINING = {
            22919, // Recall - House
            25520  // Escape House
        };

        public override void OnActivateSuccess(WorldEntity me, WorldEntity activator)
        {
            base.OnActivateSuccess(me, activator);

            if (!(activator is Player player))
                return;

            Residence residence = GlobalResidenceManager.Instance.GetResidenceByOwner(player.Name);
            if (residence == null)
                residence = GlobalResidenceManager.Instance.CreateResidence(player);

            foreach (uint spellBaseId in CONST_SPELL_TRAINING)
                if (player.SpellManager.GetSpell(spellBaseId) == null)
                    player.SpellManager.AddSpell(spellBaseId);

            ResidenceEntrance entrance = GlobalResidenceManager.Instance.GetResidenceEntrance(residence.PropertyInfoId);
            player.Rotation = entrance.Rotation.ToEulerDegrees();
            player.TeleportTo(entrance.Entry, entrance.Position, residence.Id);
        }
    }
}
