using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell.Event;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System.Linq;

namespace NexusForever.WorldServer.Game.Spell
{
    [SpellType(CastMethod.ClientSideInteraction)]
    public partial class SpellClientSideInteraction : Spell, ISpell
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static CastMethod castMethod = CastMethod.ClientSideInteraction;

        public SpellClientSideInteraction(UnitEntity caster, SpellParameters parameters) 
            : base(caster, parameters, castMethod)
        {
        }

        public override bool Cast()
        {
            if (!base.Cast())
                return false;

            double castTime = parameters.CastTimeOverride > 0 ? parameters.CastTimeOverride / 1000d : parameters.SpellInfo.Entry.CastTime / 1000d;
            if ((CastMethod)parameters.SpellInfo.BaseInfo.Entry.CastMethod != CastMethod.ClientSideInteraction)
                events.EnqueueEvent(new SpellEvent(castTime, SucceedClientInteraction));

            status = SpellStatus.Casting;
            log.Trace($"Spell {parameters.SpellInfo.Entry.Id} has started casting.");
            return true;
        }

        protected override bool _IsCasting()
        {
            return base._IsCasting() && status == SpellStatus.Casting;
        }

        private void SendSpellStartClientInteraction()
        {
            // Shoule we actually emit client interaction events to everyone? - Logs suggest that we only see this packet firing when the client interacts with -something- and is likely only sent to them
            if (caster is Player player)
            {
                player.Session.EnqueueMessageEncrypted(new ServerSpellStartClientInteraction
                {
                    ClientUniqueId = parameters.ClientSideInteraction.ClientUniqueId,
                    CastingId      = CastingId,
                    CasterId       = GetPrimaryTargetId()
                });
            }
        }

        /// <summary>
        /// Used when a <see cref="CSI.ClientSideInteraction"/> succeeds
        /// </summary>
        public void SucceedClientInteraction()
        {
            Execute();

            if (parameters.SpellInfo.Effects.FirstOrDefault(x => (SpellEffectType)x.EffectType == SpellEffectType.Activate) == null)
                parameters.ClientSideInteraction.TriggerSuccess();
        }

        /// <summary>
        /// Used when a <see cref="CSI.ClientSideInteraction"/> fails
        /// </summary>
        public void FailClientInteraction()
        {
            parameters.ClientSideInteraction.TriggerFail();

            CancelCast(CastResult.ClientSideInteractionFail);
        }

        protected override void OnStatusChange(SpellStatus previousStatus, SpellStatus status)
        {
            switch (status)
            {
                case SpellStatus.Casting:
                    if (parameters.ClientSideInteraction.Entry != null)
                        SendSpellStart();
                    else
                        SendSpellStartClientInteraction();
                    break;
            }
        }

        protected override uint GetPrimaryTargetId()
        {
            return parameters.ClientSideInteraction.Entry != null ? caster.Guid : parameters.PrimaryTargetId;
        }
    }
}
