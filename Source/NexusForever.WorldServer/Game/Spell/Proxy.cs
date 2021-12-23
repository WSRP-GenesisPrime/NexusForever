using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Prerequisite;
using NexusForever.WorldServer.Game.Spell.Event;
using System;

namespace NexusForever.WorldServer.Game.Spell
{
    public class Proxy
    {
        public UnitEntity Target { get; }
        public Spell4EffectsEntry Entry { get; }
        public Spell ParentSpell { get; }
        public bool CanCast { get; private set; } = false;

        private SpellParameters proxyParameters;

        public Proxy (UnitEntity target, Spell4EffectsEntry entry, Spell parentSpell, SpellParameters parameters)
        {
            Target = target;
            Entry = entry;
            ParentSpell = parentSpell;

            proxyParameters = new SpellParameters
            {
                ParentSpellInfo = parameters.SpellInfo,
                RootSpellInfo = parameters.RootSpellInfo,
                PrimaryTargetId = Target.Guid,
                UserInitiatedSpellCast = parameters.UserInitiatedSpellCast,
                IsProxy = true
            };
        }

        public void Evaluate()
        {
            if (Target is not Player)
                CanCast = true;

            if (Entry.DataBits06 == 0)
                CanCast = true;

            if (CanCast)
                return;

            if (PrerequisiteManager.Instance.Meets(Target as Player, Entry.DataBits06))
                CanCast = true;
        }

        public void Cast(UnitEntity caster, SpellEventManager events)
        {
            if (!CanCast)
                return;

            if (ParentSpell.CastMethod == Static.CastMethod.Aura && Entry.TickTime > 0)
            {
                caster.CastSpell(Entry.DataBits01, proxyParameters);
                return;
            }

            events.EnqueueEvent(new SpellEvent(Entry.DelayTime / 1000d, () =>
            {
                if (Entry.TickTime > 0)
                {
                    double tickTime = Entry.TickTime;
                    if (Entry.DurationTime > 0)
                    {
                        for (int i = 1; i >= Entry.DurationTime / tickTime; i++)
                            events.EnqueueEvent(new SpellEvent(tickTime * i / 1000d, () =>
                            {
                                caster.CastSpell(Entry.DataBits01, proxyParameters);
                            }));
                    }
                    else
                        events.EnqueueEvent(TickingEvent(tickTime, () =>
                        {
                            caster.CastSpell(Entry.DataBits01, proxyParameters);
                        }));
                }
                else
                    caster.CastSpell(Entry.DataBits00, proxyParameters);
            }));
        }

        private SpellEvent TickingEvent(double tickTime, Action action)
        {
            return new SpellEvent(tickTime / 1000d, () =>
            {
                action.Invoke();
                TickingEvent(tickTime, action);
            });
        }
    }
}
