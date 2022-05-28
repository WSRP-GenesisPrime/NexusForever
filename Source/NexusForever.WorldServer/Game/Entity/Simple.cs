using System.Linq;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Entity.Network.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using EntityModel = NexusForever.Database.World.Model.EntityModel;
using NexusForever.WorldServer.Game.Reputation.Static;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Game.Entity
{
    [DatabaseEntity(EntityType.Simple)]
    public class Simple : UnitEntity
    {
        public byte QuestChecklistIdx { get; private set; }

        private EntityScript script = null;

        public Simple()
            : base(EntityType.Simple)
        {
        }

        public Simple(Creature2Entry entry, long propId, ushort plugId)
            : base(EntityType.Simple)
        {
            CreatureId = entry.Id;
            DecorPropId = propId;
            DecorPlugId = plugId;
            QuestChecklistIdx = 255;
            Faction1 = (Faction)entry.FactionId;
            Faction2 = (Faction)entry.FactionId;

            Creature2DisplayGroupEntryEntry displayGroupEntry = GameTableManager.Instance.Creature2DisplayGroupEntry.Entries.FirstOrDefault(i => i.Creature2DisplayGroupId == entry.Creature2DisplayGroupId);
            if (displayGroupEntry != null)
                DisplayInfo = displayGroupEntry.Creature2DisplayInfoId;

            CreateFlags |= EntityCreateFlag.SpawnAnimation;
            SetScript();
        }

        public override void Initialise(EntityModel model)
        {
            base.Initialise(model);
            QuestChecklistIdx = model.QuestChecklistIdx;
        }

        private void SetScript()
        {
            switch(this.CreatureId)
            {
                case 65852: // Housing - Decor - Doors - Gothic Gate
                case 70052: // Housing - Decor - Activated - Door
                case 75398: // Redmoon Door (Circular) - Decor - Housing Active Prop
                    script = new DoorScript(this);
                    break;
            }
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new SimpleEntityModel
            {
                CreatureId        = CreatureId,
                QuestChecklistIdx = QuestChecklistIdx
            };
        }

        public override ServerEntityCreate BuildCreatePacket()
        {
            ServerEntityCreate entityCreate = base.BuildCreatePacket();

            if (DecorPlugId > 0 || DecorPropId > 0)
            {
                entityCreate.WorldPlacementData = new ServerEntityCreate.WorldPlacement
                {
                    Type = 1,
                    ActivePropId = DecorPropId,
                    SocketId = DecorPlugId
                };
            }   

            return entityCreate;
        }

        public override void OnActivate(Player activator)
        {
            Creature2Entry entry = GameTableManager.Instance.Creature2.GetEntry(CreatureId);
            if (entry.DatacubeId != 0u)
                activator.DatacubeManager.AddDatacube((ushort)entry.DatacubeId, int.MaxValue);

            script?.OnActivate(activator);
        }

        public override void OnActivateCast(Player activator)
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

            //TODO: cast "116,Generic Quest Spell - Activating - Activate - Tier 1" by 0x07FD

            script?.OnActivateCast(activator);
        }

        public override void AddVisible(GridEntity entity)
        {
            base.AddVisible(entity);

            script?.OnVisible(entity);
        }
    }

    public abstract class EntityScript
    {
        public abstract void OnActivateCast(Player activator);

        public abstract void OnActivate(Player activator);

        public abstract void OnVisible(GridEntity entity);
    }

    public class DoorScript : EntityScript
    {
        public DoorScript(Simple owner)
        {
            this.owner = owner;
        }

        private bool open = false;
        private Simple owner = null;

        public override void OnActivate(Player activator)
        {
            
        }

        public override void OnActivateCast(Player activator)
        {
            open = !open;

            // Emit from Player due to way Decor Entities are tracked on the map being... different.
            activator.EnqueueToVisible(new ServerEmote()
            {
                StandState = open ? StandState.State1 : StandState.State0,
                Guid = owner.Guid
            }, true);
        }

        public override void OnVisible(GridEntity entity)
        {
            if(entity is Player player)
            {
                player.Session.EnqueueMessageEncrypted(new ServerEmote()
                {
                    StandState = open ? StandState.State1 : StandState.State0,
                    Guid = owner.Guid
                });
            }
        }
    }
}
