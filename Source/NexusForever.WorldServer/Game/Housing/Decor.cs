using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Housing.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.Shared.GameTable;
using static NexusForever.WorldServer.Network.Message.Model.ServerHousingResidenceDecor;

namespace NexusForever.WorldServer.Game.Housing
{
    public class Decor : ISaveCharacter, IBuildable<ServerHousingResidenceDecor.Decor>
    {
        public ulong Id => Residence.Id;
        public long DecorId { get; protected set; }
        public HousingDecorInfoEntry Entry { get; protected set; }

        public uint DecorInfoId => Entry?.Id ?? decorInfoId;
        private uint decorInfoId;

        public WorldEntity Entity { get; private set; }

        public DecorType Type
        {
            get => type;
            set
            {
                type = value;
                saveMask |= DecorSaveMask.Type;
            }
        }

        private DecorType type;

        public uint PlotIndex
        {
            get => plotIndex;
            set
            {
                plotIndex = value;
                saveMask |= DecorSaveMask.PlotIndex;
            }
        }

        private uint plotIndex;

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                saveMask |= DecorSaveMask.Position;
            }
        }

        private Vector3 position;

        public Quaternion Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                saveMask |= DecorSaveMask.Rotation;
            }
        }

        private Quaternion rotation;

        public float Scale
        {
            get => scale;
            set
            {
                scale = value;
                saveMask |= DecorSaveMask.Scale;
            }
        }

        private float scale;

        public ulong DecorParentId
        {
            get => decorParentId;
            set
            {
                decorParentId = value;
                saveMask |= DecorSaveMask.DecorParentId;
            }
        }

        private ulong decorParentId;

        public ushort ColourShiftId
        {
            get => colourShiftId;
            set
            {
                colourShiftId = value;
                saveMask |= DecorSaveMask.ColourShiftId;
            }
        }

        private ushort colourShiftId;

        public Residence Residence { get; protected set; }

        public uint HookBagIndex 
        {
            get => hookBagIndex;
            set
            {
                hookBagIndex = value;
                saveMask |= DecorSaveMask.Hooks;
            }
        }
        private uint hookBagIndex;

        public uint HookIndex 
        {
            get => hookIndex;
            set
            {
                hookIndex = value;
                saveMask |= DecorSaveMask.Hooks;
            }
        }
        private uint hookIndex;

        private DecorSaveMask saveMask;

        public Decor()
        {
        }

        /// <summary>
        /// Returns if <see cref="Decor"/> is enqueued to be saved to the database.
        /// </summary>
        public bool PendingCreate => (saveMask & DecorSaveMask.Create) != 0;

        /// <summary>
        /// Returns if <see cref="Decor"/> is enqueued to be deleted from the database.
        /// </summary>
        public bool PendingDelete => (saveMask & DecorSaveMask.Delete) != 0;

        /// <summary>
        /// Enqueue <see cref="Decor"/> to be deleted from the database.
        /// </summary>
        public void EnqueueDelete()
        {
            if (PendingCreate)
            {
                return; // This does not exist in the database yet, no need to delete it from the database.
            }
            saveMask = DecorSaveMask.Delete;
        }

        /// <summary>
        /// Create a new <see cref="Decor"/> from an existing database model.
        /// </summary>
        public Decor(Residence residence, ResidenceDecor model, HousingDecorInfoEntry entry)
        {
            DecorId       = (long)model.DecorId;
            type          = (DecorType)model.DecorType;

            if (type != DecorType.InteriorDecoration)
                Entry = GameTableManager.Instance.HousingDecorInfo.GetEntry(model.DecorInfoId);
            else
                decorInfoId = model.DecorInfoId;

            plotIndex     = model.PlotIndex;
            position      = new Vector3(model.X, model.Y, model.Z);
            rotation      = new Quaternion(model.Qx, model.Qy, model.Qz, model.Qw);
            scale         = model.Scale;
            decorParentId = model.DecorParentId;
            colourShiftId = model.ColourShiftId;
            hookBagIndex  = model.HookBagIndex;
            hookIndex     = model.HookIndex;
            Residence     = residence;

            saveMask = DecorSaveMask.None;
        }

        /// <summary>
        /// Create a new <see cref="Decor"/> from a <see cref="HousingDecorInfoEntry"/> template.
        /// </summary>
        public Decor(Residence residence, long decorId, HousingDecorInfoEntry entry)
        {
            DecorId   = decorId;
            Entry     = entry;
            type      = DecorType.Crate;
            position  = Vector3.Zero;
            rotation  = Quaternion.Identity;
            Residence = residence;

            saveMask = DecorSaveMask.Create;
        }
        public Decor(Residence residence, long decorId, HousingDecorInfoEntry entry, ushort color)
        {
            DecorId = decorId;
            Entry = entry;
            type = DecorType.Crate;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            Residence = residence;
            colourShiftId = color;

            saveMask = DecorSaveMask.Create;
        }

        /// <summary>
        /// Create a new <see cref="Decor"/> from a <see cref="HousingWallpaperInfoEntry"/> template. Used for Interior Remodelling.
        /// </summary>
        public Decor(long decorId, HousingWallpaperInfoEntry entry, uint hookBagIndex, uint hookIndex)
        {
            DecorId = decorId;
            decorInfoId = entry.Id;
            type = DecorType.InteriorDecoration;
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            HookBagIndex = hookBagIndex;
            HookIndex = hookIndex;
            plotIndex = 0;
            
            saveMask = DecorSaveMask.Create;
        }

        /// <summary>
        /// Create a new <see cref="Decor"/> from an existing <see cref="Decor"/>.
        /// </summary>
        /// <remarks>
        /// Copies all data from the source <see cref="Decor"/> with a new id.
        /// </remarks>
        public Decor(Residence residence, Decor decor, long decorId)
        {
            DecorId       = decorId;
            Entry         = decor.Entry;
            type          = decor.Type;
            plotIndex     = decor.PlotIndex;
            position      = decor.Position;
            rotation      = decor.Rotation;
            scale         = decor.Scale;
            decorParentId = decor.DecorParentId;
            colourShiftId = decor.ColourShiftId;
            Residence     = residence;

            saveMask = DecorSaveMask.Create;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask == DecorSaveMask.None)
                return;

            if ((saveMask & DecorSaveMask.Create) != 0)
            {
                // decor doesn't exist in database, all infomation must be saved
                context.Add(new ResidenceDecor
                {
                    Id            = Id,
                    DecorId       = (ulong)DecorId,
                    DecorInfoId   = DecorInfoId,
                    DecorType     = (uint)Type,
                    PlotIndex     = PlotIndex,
                    X             = Position.X,
                    Y             = Position.Y,
                    Z             = Position.Z,
                    Qx            = Rotation.X,
                    Qy            = Rotation.Y,
                    Qz            = Rotation.Z,
                    Qw            = Rotation.W,
                    Scale         = Scale,
                    DecorParentId = DecorParentId,
                    ColourShiftId = ColourShiftId,
                    HookBagIndex  = HookBagIndex,
                    HookIndex     = HookIndex
                });
            }
            else if ((saveMask & DecorSaveMask.Delete) != 0)
            {
                var model = new ResidenceDecor
                {
                    Id      = Id,
                    DecorId = (ulong)DecorId
                };

                context.Entry(model).State = EntityState.Deleted;
            }
            else
            {
                // decor already exists in database, save only data that has been modified
                var model = new ResidenceDecor
                {
                    Id      = Id,
                    DecorId = (ulong)DecorId
                };

                // could probably clean this up with reflection, works for the time being
                EntityEntry<ResidenceDecor> entity = context.Attach(model);
                if ((saveMask & DecorSaveMask.Type) != 0)
                {
                    model.DecorType = (uint)Type;
                    entity.Property(p => p.DecorType).IsModified = true;
                }
                if ((saveMask & DecorSaveMask.PlotIndex) != 0)
                {
                    model.PlotIndex = PlotIndex;
                    entity.Property(p => p.PlotIndex).IsModified = true;
                }
                if ((saveMask & DecorSaveMask.Position) != 0)
                {
                    model.X = Position.X;
                    entity.Property(p => p.X).IsModified = true;
                    model.Y = Position.Y;
                    entity.Property(p => p.Y).IsModified = true;
                    model.Z = Position.Z;
                    entity.Property(p => p.Z).IsModified = true;
                }
                if ((saveMask & DecorSaveMask.Rotation) != 0)
                {
                    model.Qx = Rotation.X;
                    entity.Property(p => p.Qx).IsModified = true;
                    model.Qy = Rotation.Y;
                    entity.Property(p => p.Qy).IsModified = true;
                    model.Qz = Rotation.Z;
                    entity.Property(p => p.Qz).IsModified = true;
                    model.Qw = Rotation.W;
                    entity.Property(p => p.Qw).IsModified = true;
                }
                if ((saveMask & DecorSaveMask.Scale) != 0)
                {
                    model.Scale = Scale;
                    entity.Property(p => p.Scale).IsModified = true;
                }
                if ((saveMask & DecorSaveMask.DecorParentId) != 0)
                {
                    model.DecorParentId = DecorParentId;
                    entity.Property(p => p.DecorParentId).IsModified = true;
                }
                if ((saveMask & DecorSaveMask.ColourShiftId) != 0)
                {
                    model.ColourShiftId = ColourShiftId;
                    entity.Property(p => p.ColourShiftId).IsModified = true;
                }
                if ((saveMask & DecorSaveMask.Hooks) != 0)
                {
                    model.HookBagIndex = HookBagIndex;
                    entity.Property(p => p.HookBagIndex).IsModified = true;

                    model.HookIndex = HookIndex;
                    entity.Property(p => p.HookIndex).IsModified = true;
                }
            }

            saveMask = DecorSaveMask.None;
        }

        /// <summary>
        /// Move <see cref="Decor"/> to supplied position.
        /// </summary>
        public void Move(DecorType type, Vector3 position, Quaternion rotation, float scale, uint plotIndex)
        {
            Type      = type;
            Position  = position;
            Rotation  = rotation;
            Scale     = scale;
            PlotIndex = plotIndex;
        }

        /// <summary>
        /// Move <see cref="Decor"/> to the crate.
        /// </summary>
        public void Crate()
        {
            Move(DecorType.Crate, Vector3.Zero, Quaternion.Identity, 0f, int.MaxValue);
            DecorParentId = 0u;
            RemoveEntity();
        }

        public void SetEntity(WorldEntity entity)
        {
            if (Entity != null && entity != null)
                throw new InvalidOperationException($"Cannot add an Entity to this Decor when an Entity already exists.");

            Entity = entity;
        }

        public void RemoveEntity()
        {
            if (Entity != null && Entity.Map != null)
            {
                Entity.Map.EnqueueRemove(Entity);
                Entity = null;
            }
        }

        public ServerHousingResidenceDecor.Decor Build()
        {
            return new()
            {
                RealmId       = WorldServer.RealmId,
                DecorId       = DecorId,
                ResidenceId   = Residence.Id,
                DecorType     = Type,
                PlotIndex     = PlotIndex,
                HookBagIndex  = HookBagIndex,
                HookIndex     = HookIndex,
                Scale         = Scale,
                Position      = Position,
                Rotation      = Rotation,
                DecorInfoId   = DecorInfoId,
                ParentDecorId = DecorParentId,
                ColourShift   = ColourShiftId,
                ActivePropUnitId = Entity?.EntityId ?? 0u
            };
        }
    }
}
