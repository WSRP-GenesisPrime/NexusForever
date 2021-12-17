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

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.Simple)]
    public class Simple : UnitEntity
    {
        public Simple()
            : base(EntityType.Simple)
        {
        }

        public Simple(uint creatureId)
            : base(EntityType.Simple)
        {
            CreatureId = creatureId;

            // temp
            DisplayInfo = 24413;
            SetBaseProperty(Property.BaseHealth, 800.0f);

            SetStat(Stat.Health, 800u);
            SetStat(Stat.Level, 3u);
            SetStat(Stat.Sheathed, 800u);
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
    }
}
