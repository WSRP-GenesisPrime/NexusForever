using System;
using System.Collections.Generic;
using System.Linq;
using NexusForever.Database.World.Model;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Game.Static;
using NLog;

namespace NexusForever.WorldServer.Game.Entity
{
    public abstract class UnitEntity : WorldEntity
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly List<Spell.Spell> pendingSpells = new();
        private Dictionary<ProcType, List<ProcInfo>> procs = new();

        public float HitRadius { get; protected set; } = 1f;

        protected UnitEntity(EntityType type)
            : base(type)
        {
            InitialiseHitRadius();
            foreach (ProcType procType in AssetManager.HandledProcTypes)
                procs.Add(procType, new List<ProcInfo>());
        }

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);
        }

        private void InitialiseHitRadius()
        {
            if (CreatureId == 0u)
                return;

            Creature2Entry creatureEntry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);
            if (creatureEntry == null)
                return;

            Creature2ModelInfoEntry modelInfoEntry = GameTableManager.Instance.Creature2ModelInfo.GetEntry(creatureEntry.Creature2ModelInfoId);
            if (modelInfoEntry != null)
                HitRadius = modelInfoEntry.HitRadius * creatureEntry.ModelScale;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            foreach (Spell.Spell spell in pendingSpells.ToArray())
            {
                spell.Update(lastTick);
                if (spell.IsFinished)
                    pendingSpells.Remove(spell);
            }

            foreach (ProcInfo proc in procs.Values.SelectMany(p => p).ToList())
                proc.Update(lastTick);
        }

        /// <summary>
        /// Cast a <see cref="Spell"/> with the supplied spell id and <see cref="SpellParameters"/>.
        /// </summary>
        public void CastSpell(uint spell4Id, SpellParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException();

            Spell4Entry spell4Entry = GameTableManager.Instance.Spell4.GetEntry(spell4Id);
            if (spell4Entry == null)
                throw new ArgumentOutOfRangeException();

            CastSpell(spell4Entry.Spell4BaseIdBaseSpell, (byte)spell4Entry.TierIndex, parameters);
        }

        /// <summary>
        /// Cast a <see cref="Spell"/> with the supplied spell base id, tier and <see cref="SpellParameters"/>.
        /// </summary>
        public void CastSpell(uint spell4BaseId, byte tier, SpellParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException();

            SpellBaseInfo spellBaseInfo = GlobalSpellManager.Instance.GetSpellBaseInfo(spell4BaseId);
            if (spellBaseInfo == null)
                throw new ArgumentOutOfRangeException();

            SpellInfo spellInfo = spellBaseInfo.GetSpellInfo(tier);
            if (spellInfo == null)
                throw new ArgumentOutOfRangeException();

            parameters.SpellInfo = spellInfo;
            CastSpell(parameters);
        }

        /// <summary>
        /// Cast a <see cref="Spell"/> with the supplied <see cref="SpellParameters"/>.
        /// </summary>
        public void CastSpell(SpellParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException();

            if (DisableManager.Instance.IsDisabled(DisableType.BaseSpell, parameters.SpellInfo.BaseInfo.Entry.Id))
            {
                if (this is Player player)
                    player.SendSystemMessage($"Unable to cast base spell {parameters.SpellInfo.BaseInfo.Entry.Id} because it is disabled.");
                return;
            }

            if (DisableManager.Instance.IsDisabled(DisableType.Spell, parameters.SpellInfo.Entry.Id))
            {
                if (this is Player player)
                    player.SendSystemMessage($"Unable to cast spell {parameters.SpellInfo.Entry.Id} because it is disabled.");
                return;
            }

            if (parameters.UserInitiatedSpellCast)
            {
                if (this is Player player)
                    player.Dismount();
            }

            var spell = new Spell.Spell(this, parameters);
            spell.Cast();
            pendingSpells.Add(spell);
        }

        /// <summary>
        /// Cancel any <see cref="Spell"/>'s that are interrupted by movement.
        /// </summary>
        public void CancelSpellsOnMove()
        {
            foreach (Spell.Spell spell in pendingSpells)
                if (spell.IsMovingInterrupted() && spell.IsCasting)
                    spell.CancelCast(CastResult.CasterMovement);
        }

        /// <summary>
        /// Cancel a <see cref="Spell"/> based on its casting id
        /// </summary>
        /// <param name="castingId">Casting ID of the spell to cancel</param>
        public void CancelSpellCast(uint castingId)
        {
            Spell.Spell spell = pendingSpells.SingleOrDefault(s => s.CastingId == castingId);
            spell?.CancelCast(CastResult.SpellCancelled);
        }

        /// <summary>
        /// 
        /// </summary>
        public ProcInfo ApplyProc(Spell4EffectsEntry entry)
        {
            ProcInfo proc = new ProcInfo(this, entry);

            if (procs.ContainsKey(proc.Type))
            {
                bool canApplyProc = true;
                foreach (ProcInfo procInfo in procs[proc.Type])
                {
                    if (procInfo.ApplicatorSpell4Id == proc.ApplicatorSpell4Id)
                        canApplyProc = false;
                }

                if (canApplyProc)
                    procs[proc.Type].Add(proc);
                else
                    return null;
            }
            else
            {
                log.Warn($"Unhandled ProcType {entry.DataBits00}!");
                return null;
            }
            return proc;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveProc(uint spell4Id)
        {
            foreach (List<ProcInfo> procList in procs.Values.ToList())
            {
                foreach (ProcInfo proc in procList.ToList())
                {
                    if (proc.ApplicatorSpell4Id == spell4Id)
                    {
                        procs[proc.Type].Remove(proc);
                        log.Trace($"Removed Proc {proc.Effect.Id} from {proc.Type} for Entity {Guid}.");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void FireProc(ProcType type)
        {
            if (procs.TryGetValue(type, out List<ProcInfo> procList))
            {
                foreach (ProcInfo proc in procList)
                    proc.Trigger();
            }
        }
    }
}
