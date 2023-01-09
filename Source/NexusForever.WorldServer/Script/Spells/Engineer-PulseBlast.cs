using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusForever.WorldServer.Script.Spells
{
    [Script(22522)] // Engineer - Pulse Blast Damage
    public class Engineer_PulseBlast : SpellScript
    {
        private readonly Dictionary</* castingId*/ uint, /*hasAppliedVolatility*/ bool> appliedDict = new();
        const uint SPELL_HIDDEN_VOLATILITY_15 = 42148;

        public override void OnExecute(Spell spell, SpellParameters parameters, UnitEntity caster, uint currentPhase, IEnumerable<uint> uniqueTargetIds)
        {
            if (appliedDict.TryGetValue(spell.CastingId, out bool hasApplied))
            {
                if (currentPhase == 3)
                    appliedDict.Remove(spell.CastingId);

                return;
            }

            bool applyVolatility = false;
            foreach (uint targetId in uniqueTargetIds)
            {
                if (targetId == caster.Guid)
                    continue;

                WorldEntity targetEntity = caster.GetVisible<WorldEntity>(targetId);
                if (targetEntity == null)
                    continue;

                if (targetEntity is not UnitEntity targetUnit)
                    continue;

                if (!caster.CanAttack(targetUnit))
                    continue;

                applyVolatility = true;
                break;
            }

            if (applyVolatility)
            {
                caster.CastSpell(SPELL_HIDDEN_VOLATILITY_15, new SpellParameters
                {
                    PrimaryTargetId        = caster.Guid,
                    UserInitiatedSpellCast = parameters.UserInitiatedSpellCast,
                    IsProxy                = true
                });
                appliedDict.TryAdd(spell.CastingId, true);
            }
        }
    }
}
