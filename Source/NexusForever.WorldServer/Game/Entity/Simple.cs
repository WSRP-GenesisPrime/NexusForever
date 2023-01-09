using System;
using System.Linq;
using System.Numerics;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Reputation.Static;
using NexusForever.WorldServer.Script;
using EntityModel = NexusForever.Database.World.Model.EntityModel;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.Simple)]
    public class Simple : UnitEntity
    {
        public Action<Simple> afterAddToMap;

        private EntityScript script = null;

        public Simple()
            : base(EntityType.Simple)
        {
        }

        public Simple(uint creatureId, Action<Simple> actionAfterAddToMap = null)
            : base(EntityType.Simple)
        {
            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(creatureId);
            if (entry == null)
                throw new ArgumentNullException();

            CreatureId = creatureId;
            afterAddToMap = actionAfterAddToMap;

            SetBaseProperty(Property.BaseHealth, 101.0f);

            SetStat(Stat.Health, 101u);
            SetStat(Stat.Level, 1u);

            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Instance.
                Creature2DisplayGroupEntry.
                Entries.
                FirstOrDefault(d => d.Creature2DisplayGroupId == entry.Creature2DisplayGroupId);
            if (displayGroupEntry != null)
                DisplayInfo = displayGroupEntry.Creature2DisplayInfoId;
        }

        public Simple(Creature2Entry entry, long propId, ushort plugId)
            : base(EntityType.Simple)
        {
            CreatureId = entry.Id;
            ActivePropId = propId;
            WorldSocketId = plugId;
            QuestChecklistIdx = 255;
            Faction1 = (Faction)entry.FactionId;
            Faction2 = (Faction)entry.FactionId;

            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Instance.Creature2DisplayGroupEntry.Entries.FirstOrDefault(i => i.Creature2DisplayGroupId == entry.Creature2DisplayGroupId);
            if (displayGroupEntry != null)
                DisplayInfo = displayGroupEntry.Creature2DisplayInfoId;

            CreateFlags |= EntityCreateFlag.SpawnAnimation;
        }

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);
            if (Health == 0u)
            {
                MaxHealth = 101u;
                ModifyHealth((long)MaxHealth);
            }
            QuestChecklistIdx = model.QuestChecklistIdx;

            ScriptManager.Instance.GetScript<CreatureScript>(CreatureId)?.OnCreate(this);
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            afterAddToMap?.Invoke(this);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new SimpleEntityModel
            {
                CreatureId        = CreatureId,
                QuestChecklistIdx = QuestChecklistIdx
            };
        }

        public override void OnInteract(Player activator)
        {
            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);
            if (entry.DatacubeId != 0u)
                activator.DatacubeManager.AddDatacube((ushort)entry.DatacubeId, int.MaxValue);

            ScriptManager.Instance.GetScript<CreatureScript>(CreatureId)?.OnActivate(this, activator);
        }

        public override void OnActivateSuccess(Player activator)
        {
            uint progress = (uint)(1 << QuestChecklistIdx);

            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);
            if (entry.DatacubeId != 0u)
            {
                Datacube datacube = activator.DatacubeManager.GetDatacube((ushort)entry.DatacubeId, DatacubeType.Datacube);
                if (datacube == null)
                    activator.DatacubeManager.AddDatacube((ushort)entry.DatacubeId, progress);
                else
                {
                    datacube.Progress |= progress;
                    activator.DatacubeManager.SendDatacube(datacube);
                }
            }

            if (entry.DatacubeVolumeId != 0u)
            {
                Datacube datacube = activator.DatacubeManager.GetDatacube((ushort)entry.DatacubeVolumeId, DatacubeType.Journal);
                if (datacube == null)
                    activator.DatacubeManager.AddDatacubeVolume((ushort)entry.DatacubeVolumeId, progress);
                else
                {
                    datacube.Progress |= progress;
                    activator.DatacubeManager.SendDatacubeVolume(datacube);
                }
            }

            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateEntity, CreatureId, 1u);
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.SucceedCSI, CreatureId, 1u);
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateTargetGroupChecklist, CreatureId, QuestChecklistIdx);
            
            ScriptManager.Instance.GetScript<CreatureScript>(CreatureId)?.OnActivateSuccess(this, activator);
        }

        public override void AddVisible(GridEntity entity)
        {
            base.AddVisible(entity);

            script?.OnVisible(entity);
        }
    }

    public abstract class EntityScript
    {
        public virtual void OnActivateCast(Player activator)
        {
        }

        public virtual void OnActivate(Player activator)
        {
        }

        public virtual void OnVisible(GridEntity entity)
        {
        }
    }

    public class SimpleStateScript : EntityScript
    {
        public SimpleStateScript(Simple owner, StandState state2 = StandState.State1, StandState state1 = StandState.State0)
        {
            this.owner = owner;
            this.state1 = state1;
            this.state2 = state2;
        }

        private bool open = false;
        private Simple owner = null;
        private StandState state1;
        private StandState state2;

        public override void OnActivateCast(Player activator)
        {
            open = !open;

            // Emit from Player due to way Decor Entities are tracked on the map being... different.
            activator.EnqueueToVisible(new ServerEmote()
            {
                StandState = open ? state2 : state1,
                Guid = owner.Guid
            }, true);
        }

        public override void OnVisible(GridEntity entity)
        {
            if (entity is Player player)
            {
                player.Session.EnqueueMessageEncrypted(new ServerEmote()
                {
                    StandState = open ? state2 : state1,
                    Guid = owner.Guid
                });
            }
        }
    }
}
