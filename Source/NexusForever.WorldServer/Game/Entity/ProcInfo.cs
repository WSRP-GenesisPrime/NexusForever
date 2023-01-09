using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Spell;
using NLog;

namespace NexusForever.WorldServer.Game.Entity
{
    public class ProcInfo : IUpdate
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public UnitEntity Owner { get; }
        public uint ApplicatorSpell4Id { get; }
        public Spell4EffectsEntry Effect { get; }
        public ProcType Type { get; }

        private UpdateTimer triggerTimer;
        private uint triggerSpell4Id;

        public ProcInfo(UnitEntity owner, Spell4EffectsEntry entry)
        {
            Owner              = owner;
            Effect             = entry;
            ApplicatorSpell4Id = entry.SpellId;
            Type               = (ProcType)entry.DataBits00;

            triggerTimer       = new UpdateTimer(entry.DataBits04 / 1000d, false);
            triggerSpell4Id    = entry.DataBits01;
        }

        public void Update(double lastTick)
        {
            triggerTimer.Update(lastTick);
            if (triggerTimer.HasElapsed)
            {
                log.Trace($"Triggering Proc {Effect.Id} of {Type}.");
                Owner.CastSpell(triggerSpell4Id, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
                triggerTimer.Reset(false);
            }
        }

        /// <summary>
        /// Trigger this <see cref="ProcInfo"/>'s Spell.
        /// </summary>
        public void Trigger()
        {
            log.Warn($"Attempting to Trigger Proc {Effect.Id} of {Type}.");

            if (CanTrigger())
                triggerTimer.Reset(true);
        }

        /// <summary>
        /// End all instances of this <see cref="ProcInfo"/>'s Spell.
        /// </summary>
        public void EndTrigger()
        {
            Owner.FinishSpells(triggerSpell4Id);
        }

        private bool CanTrigger()
        {
            // TODO: Check Prerequisite values

            if (triggerTimer.IsTicking)
                return false;

            return true;
        }
    }
}
