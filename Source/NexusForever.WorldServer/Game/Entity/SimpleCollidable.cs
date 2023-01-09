using NexusForever.Database.World.Model;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Script;
using System;
using System.Numerics;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.SimpleCollidable)]
    public class SimpleCollidable : WorldEntity
    {
        private Action action;

        public SimpleCollidable()
            : base(EntityType.SimpleCollidable)
        {
        }

        public SimpleCollidable(uint creatureId)
            : base(EntityType.SimpleCollidable)
        {
            CreatureId = creatureId;

            // temp
            DisplayInfo = 24413;
            SetBaseProperty(Property.BaseHealth, 101f);

            SetStat(Stat.Health, 101u);
        }

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);
            QuestChecklistIdx = model.QuestChecklistIdx;

            ScriptManager.Instance.GetScript<CreatureScript>(CreatureId)?.OnCreate(this);
        }

        public SimpleCollidable(uint creatureId, uint displayInfoId, Action action = null, byte questChecklistIdx = 255)
            : base(EntityType.SimpleCollidable)
        {
            CreatureId = creatureId;
            DisplayInfo = displayInfoId;
            this.action = action;
            QuestChecklistIdx = questChecklistIdx;

            Properties.Add(Property.BaseHealth, new PropertyValue(Property.BaseHealth, 101f, 101f));
            stats.Add(Stat.Health, new StatValue(Stat.Health, 101u));
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new SimpleCollidableEntityModel
            {
                CreatureId        = CreatureId,
                QuestChecklistIdx = QuestChecklistIdx
            };
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            if (action != null)
                action.Invoke();
        }
    }
}
