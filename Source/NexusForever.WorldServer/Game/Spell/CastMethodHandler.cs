using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using System;
using System.Linq;

namespace NexusForever.WorldServer.Game.Spell
{
    public delegate void CastMethodDelegate(Spell spell);

    public partial class Spell
    {
        [CastMethodHandler(CastMethod.Normal)]
        private void NormalHandler()
        {
            uint castTime = parameters.CastTimeOverride > -1 ? (uint)parameters.CastTimeOverride : parameters.SpellInfo.Entry.CastTime;

            events.EnqueueEvent(new SpellEvent(castTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
        }

        [CastMethodHandler(CastMethod.Multiphase)]
        private void MultiphaseHandler()
        {
            status = SpellStatus.Executing;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started executing.");

            uint spellDelay = 0;
            for(int i = 0; i < parameters.SpellInfo.Phases.Count; i++)
            {
                int index = i;
                SpellPhaseEntry spellPhase = parameters.SpellInfo.Phases[i];
                spellDelay += spellPhase.PhaseDelay;
                events.EnqueueEvent(new SpellEvent(spellDelay / 1000d, () =>
                {
                    currentPhase = (byte)spellPhase.OrderIndex;
                    effectTriggerCount.Clear();
                    Execute();

                    //targets.ForEach(t => t.Effects.Clear());

                    if (i == parameters.SpellInfo.Phases.Count - 1)
                    {
                        status = SpellStatus.Finishing;
                        log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has finished executing.");
                    }
                }));
            }

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
        }

        [CastMethodHandler(CastMethod.Channeled)]
        [CastMethodHandler(CastMethod.ChanneledField)]
        private void ChanneledHandler()
        {
            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.ChannelInitialDelay / 1000d, () =>
            {
                CastResult checkResources = CheckResourceConditions();
                if (checkResources != CastResult.Ok)
                {
                    CancelCast(checkResources);
                    return;
                }

                Execute();

                targets.ForEach(t => t.Effects.Clear());
            })); // Execute after initial delay
            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.ChannelMaxTime / 1000d, Finish)); // End Spell Cast

            uint numberOfPulses = (uint)MathF.Floor(parameters.SpellInfo.Entry.ChannelMaxTime / parameters.SpellInfo.Entry.ChannelPulseTime); // Calculate number of "ticks" in this spell cast

            // Add ticks at each pulse
            for (int i = 1; i <= numberOfPulses; i++)
                events.EnqueueEvent(new SpellEvent((parameters.SpellInfo.Entry.ChannelInitialDelay + (parameters.SpellInfo.Entry.ChannelPulseTime * i)) / 1000d, () =>
                {
                    CastResult checkResources = CheckResourceConditions();
                    if (checkResources != CastResult.Ok)
                    {
                        CancelCast(checkResources);
                        return;
                    }

                    effectTriggerCount.Clear();
                    Execute();

                    targets.ForEach(t => t.Effects.Clear());
                }));

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
        }

        [CastMethodHandler(CastMethod.ChargeRelease)]
        private void ChargeReleaseHandler()
        {
            if (parameters.ParentSpellInfo == null)
            {
                totalThresholdTimer = (uint)(parameters.SpellInfo.Entry.ThresholdTime / 1000d);

                // Keep track of cast time increments as we create timers to adjust thresholdValue
                uint nextCastTime = 0;
                
                // Create timers for each thresholdEntry's timer increment
                foreach (Spell4ThresholdsEntry thresholdsEntry in parameters.SpellInfo.Thresholds)
                {
                    nextCastTime += thresholdsEntry.ThresholdDuration;

                    if (thresholdsEntry.OrderIndex == 0)
                        continue;

                    events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d + nextCastTime / 1000d, () =>
                    {
                        thresholdValue = thresholdsEntry.OrderIndex;
                        SendThresholdUpdate();
                    }));
                }
            }

            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
        }

        [CastMethodHandler(CastMethod.RapidTap)]
        private void RapidTapHandler()
        {
            if (parameters.ParentSpellInfo == null)
                events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d + parameters.SpellInfo.Entry.ThresholdTime / 1000d, Finish)); // enqueue spell to be executed after cast time

            events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.CastTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
        }

        [CastMethodHandler(CastMethod.Aura)]
        private void AuraHandler()
        {
            uint castTime = parameters.CastTimeOverride > -1 ? (uint)parameters.CastTimeOverride : parameters.SpellInfo.Entry.CastTime;
            events.EnqueueEvent(new SpellEvent(castTime / 1000d, Execute)); // enqueue spell to be executed after cast time

            foreach (Spell4EffectsEntry effect in parameters.SpellInfo.Effects.Where(i => i.TickTime > 0))
            {
                if ((SpellEffectType)effect.EffectType != SpellEffectType.Proxy)
                {
                    log.Warn($"Aura (Spell4 {Spell4Id}) has unhandled effect type {(SpellEffectType)effect.EffectType}.");
                    continue;
                }
                
                effectRetriggerTimers.Add(effect.Id, effect.TickTime / 1000d);
            }

            if (parameters.SpellInfo.Entry.SpellDuration > 0 && parameters.SpellInfo.Entry.SpellDuration < uint.MaxValue)
                events.EnqueueEvent(new SpellEvent(parameters.SpellInfo.Entry.SpellDuration / 1000d, Finish));

            if (parameters.SpellInfo.BaseInfo.Entry.Creature2IdPositionalAoe > 0)
            {
                Simple positionalEntity = new Simple(parameters.SpellInfo.BaseInfo.Entry.Creature2IdPositionalAoe, (entity) =>
                {
                    entity.Rotation = caster.Rotation;
                    parameters.PositionalUnitId = entity.Guid;

                    status = SpellStatus.Casting;
                    log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
                });

                caster.Map.EnqueueAdd(positionalEntity, new Map.MapPosition
                {
                    Info = new Map.MapInfo
                    {
                        Entry = caster.Map.Entry
                    },
                    Position = caster.Position
                });
            }
            else
            {
                status = SpellStatus.Casting;
                log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
            }
        }
    }
}
