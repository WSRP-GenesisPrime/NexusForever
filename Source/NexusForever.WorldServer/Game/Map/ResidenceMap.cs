using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NexusForever.Shared.Game.Map;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.GameTable.Static;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Housing.Static;
using NexusForever.WorldServer.Game.Map.Search;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;

namespace NexusForever.WorldServer.Game.Map
{
    public class ResidenceMap : BaseMap
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public ulong Id => residence?.Id ?? 0ul;
        // housing maps have unlimited vision range.
        public override float VisionRange { get; protected set; } = -1f;
        public uint ResidenceInfoId => residence.ResidenceInfoEntry?.Id ?? 0u;

        public Residence residence { get; private set; }

        private readonly Dictionary</* decorId */ long, TemporaryDecor> instancedTemporaryDecor = new Dictionary<long, TemporaryDecor>();
        private readonly Dictionary</* decorId */ ulong, Decor> decorEntities = new Dictionary<ulong, Decor>();

        private UpdateTimer unloadTimer = new UpdateTimer(30d, false);

        public override void Initialise(MapInfo info, Player player)
        {
            base.Initialise(info, player);
            IsStatic = true;

            if (info.ResidenceId != 0u)
            {
                residence = ResidenceManager.Instance.GetCachedResidence(info.ResidenceId);
                if (residence == null)
                    throw new InvalidOperationException();
            }
            else
                residence = ResidenceManager.Instance.CreateResidence(player);

            // initialise plug entities
            foreach (Plot plot in residence.GetPlots())
            {
                plot.SetMap(this);
                
                if (plot.PlugEntry != null)
                {
                    plot.CreatePlug(null);
                    plot.HandleExtrasOrScripts();
                }
            }
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            foreach (Plot plot in residence.GetPlots())
                plot.Update(lastTick);

            foreach (Decor decor in decorEntities.Values)
                decor.Entity?.Update(lastTick);

            CheckUnloadTimer();

            if (unloadTimer.IsTicking)
                unloadTimer.Update(lastTick);

            if (unloadTimer.HasElapsed && isReadyToUnload == false)
            {
                isReadyToUnload = true;
                foreach (Plot plot in residence.GetPlots())
                    plot.SetMap(null);

                foreach (Decor decor in decorEntities.Values)
                    decor.SetEntity(null);
            }
        }

        private void CheckUnloadTimer()
        {
            if (unloadTimer.HasElapsed)
                return;

            if (entities.Values.FirstOrDefault(e => e is Player) != null)
            {
                if (unloadTimer.IsTicking)
                    unloadTimer.Reset(false);

                return;
            }

            if (!unloadTimer.IsTicking)
                unloadTimer.Reset(true);
        }

        public override void OnAddToMap(Player player)
        {
            if (residence == null)
                throw new InvalidOperationException();

            if (player.SpellManager.GetSpell(25520) == null)
                player.SpellManager.AddSpell(25520);

            if (residence.OwnerId == player.CharacterId && player.SpellManager.GetSpell(22919) == null)
                player.SpellManager.AddSpell(22919);

            SendHousingPrivacy(player);
            SendHousingProperties(player);
            SendHousingPlots(player);

            // this shows the housing toolbar, might need to move this to a more generic place in the future
            player.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet = ShortcutSet.FloatingSpellBar,
                ActionBarShortcutSetId = 1553,
                Guid = residence.GetPlot(0).GetPlotEntities().FirstOrDefault(i => i.Type == EntityType.Residence)?.Guid ?? player.Guid
            });

            SendResidenceDecor(player);
        }

        public override void OnRemoveFromMap(Player player)
        {
            player.HouseOutsideLocation = Vector3.Zero;

            foreach (Decor decor in decorEntities.Values.Where(d => d.Entity != null))
                player.RemoveVisible(decor.Entity);

            foreach (TemporaryDecor decor in instancedTemporaryDecor.Values.Where(d => d.Entity != null))
                player.RemoveVisible(decor.Entity);
        }

        private void SendHousingPrivacy(Player player = null)
        {
            var housingPrivacy = new ServerHousingPrivacy
            {
                ResidenceId     = residence.Id,
                NeighbourhoodId = 0x190000000000000A, // magic numbers are bad
                PrivacyLevel    = ResidencePrivacyLevel.Public
            };

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingPrivacy);
            else
                EnqueueToAll(housingPrivacy);
        }

        public void SendHousingProperties(Player player = null)
        {
            var housingProperties = new ServerHousingProperties
            {
                Residences =
                {
                    new ServerHousingProperties.Residence
                    {
                        RealmId           = WorldServer.RealmId,
                        ResidenceId       = residence.Id,
                        NeighbourhoodId   = 0x190000000000000A,
                        CharacterIdOwner  = residence.OwnerId,
                        Name              = residence.Name,
                        PropertyInfoId    = residence.PropertyInfoId,
                        ResidenceInfoId   = residence.ResidenceInfoEntry?.Id ?? 0u,
                        WallpaperExterior = residence.Wallpaper,
                        Entryway          = residence.Entryway,
                        Roof              = residence.Roof,
                        Door              = residence.Door,
                        Ground            = residence.Ground,
                        Music             = residence.Music,
                        Sky               = residence.Sky,
                        Flags             = residence.Flags,
                        ResourceSharing   = residence.ResourceSharing,
                        GardenSharing     = residence.GardenSharing
                    }
                }
            };

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingProperties);
            else
                EnqueueToAll(housingProperties);
        }
 
        /// <summary>
        /// Sends the list of <see cref="Decor"/> applicable for this <see cref="ResidenceMap"/> to a single <see cref="Player"/> or all Player entities.
        /// </summary>
        public void SendResidenceDecor(Player player = null)
        {
            foreach (IWritable message in GetResidenceDecorMessages())
            {
                if (player != null)
                    player.Session.EnqueueMessageEncrypted(message);
                else
                    EnqueueToAll(message);
            }
        }

        private IEnumerable<IWritable> GetResidenceDecorMessages()
        {
            List<IWritable> messages = new List<IWritable>();

            var groups = residence.GetDecor().GroupBy(r => r.Type)
                    .Select(grp => grp.ToList())
                    .ToList();
            uint houseOperation = (uint)groups.Count() - 1;

            foreach (List<Decor> decorList in groups)
            {
                var residenceDecor = new ServerHousingResidenceDecor();

                for (uint i = 0u; i < decorList.Count; i++)
                {
                    // client freaks out if too much decor is sent in a single message, limit to 100
                    if (i != 0u && i % 100u == 0u)
                    {
                        messages.Add(residenceDecor);

                        residenceDecor = new ServerHousingResidenceDecor();
                        residenceDecor.Operation = houseOperation--;
                    }

                    Decor decor = decorList[(int)i];
                    residenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
                    {
                        RealmId = WorldServer.RealmId,
                        DecorId = (ulong)decor.DecorId,
                        ResidenceId = residence.Id,
                        DecorType = decor.Type,
                        PlotIndex = decor.PlotIndex,
                        HookBagIndex = decor.HookBagIndex,
                        HookIndex = decor.HookIndex,
                        Scale = decor.Scale,
                        Position = decor.Position,
                        Rotation = decor.Rotation,
                        DecorInfoId = decor.DecorInfoId,
                        ParentDecorId = decor.DecorParentId,
                        ColourShift = decor.ColourShiftId
                    });

                    if (i == decorList.Count - 1)
                        messages.Add(residenceDecor);
                }
            }

            return messages;
        }

        /// <summary>
        /// Send the ActionBar update to all <see cref="Player"/> entities on this <see cref="ResidenceMap"/> 
        /// </summary>
        public void SendActionBars()
        {
            // TODO: Allow Macro functionality in EnqueueToAll, so we can use 1 function to emit player-based values (like player.Guid below)
            foreach (GridEntity entity in entities.Values)
            {
                Player player = entity as Player;
                player?.Session.EnqueueMessageEncrypted(new ServerShowActionBar
                {
                    ShortcutSet = ShortcutSet.FloatingSpellBar,
                    ActionBarShortcutSetId = 1553,
                    Guid = residence.GetPlot(0).GetPlotEntities().FirstOrDefault(i => i.Type == EntityType.Residence)?.Guid ?? player.Guid
                });
            }
        }

        /// <summary>
        /// Crate all placed <see cref="Decor"/>, this is called directly from a packet hander.
        /// </summary>
        public void CrateAllDecor(Player player)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            var housingResidenceDecor = new ServerHousingResidenceDecor();
            foreach (Decor decor in residence.GetPlacedDecor())
            {
                decor.Crate();

                housingResidenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
                {
                    RealmId     = WorldServer.RealmId,
                    DecorId     = (ulong)decor.DecorId,
                    ResidenceId = residence.Id,
                    DecorType   = decor.Type,
                    PlotIndex   = decor.PlotIndex,
                    Scale       = decor.Scale,
                    Position    = decor.Position,
                    Rotation    = decor.Rotation,
                    DecorInfoId = decor.DecorInfoId
                });
            }

            EnqueueToAll(housingResidenceDecor);
        }

        /// <summary>
        /// Update <see cref="Decor"/> (create, move or delete), this is called directly from a packet hander.
        /// </summary>
        public void DecorUpdate(Player player, ClientHousingDecorUpdate housingDecorUpdate)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            foreach (DecorUpdate update in housingDecorUpdate.DecorUpdates)
            {
                switch (housingDecorUpdate.Operation)
                {
                    case DecorUpdateOperation.Create:
                        DecorCreate(player, update);
                        break;
                    case DecorUpdateOperation.Move:
                        DecorMove(player, update);
                        break;
                    case DecorUpdateOperation.Delete:
                        DecorDelete(update);
                        break;
                    default:
                        throw new InvalidPacketValueException();
                }
            }
        }

        public void DecorUpdate(Player player, ClientHousingRemodelInterior remodelInterior)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            foreach (DecorUpdate update in remodelInterior.Remodels)
            {
                Decor decor = residence.GetInteriorDecor(update.HookIndex);
                if (decor != null && update.DecorInfoId == 0u)
                {
                    DecorDelete(update);
                    continue;
                }
                
                if (decor != null && update.DecorInfoId != decor.DecorInfoId)
                    DecorDelete(update);
                
                if (update.DecorInfoId == 0u)
                    continue;
                
                decor = residence.DecorCreate(update);

                EnqueueToAll(new ServerHousingResidenceDecor
                {
                    Operation = 0,
                    DecorData = new List<ServerHousingResidenceDecor.Decor>
                {
                    new ServerHousingResidenceDecor.Decor
                    {
                        RealmId     = WorldServer.RealmId,
                        DecorId     = (ulong)decor.DecorId,
                        ResidenceId = residence.Id,
                        DecorType   = decor.Type,
                        PlotIndex   = decor.PlotIndex,
                        HookBagIndex = decor.HookBagIndex,
                        HookIndex   = decor.HookIndex,
                        Scale       = decor.Scale,
                        Position    = decor.Position,
                        Rotation    = decor.Rotation,
                        DecorInfoId = decor.DecorInfoId,
                        ColourShift = decor.ColourShiftId
                    }
                }
                });
            }
        }

        /// <summary>
        /// Create and add <see cref="Decor"/> from supplied <see cref="HousingDecorInfoEntry"/> to your crate.
        /// </summary>
        public void DecorCreate(HousingDecorInfoEntry entry, uint quantity)
        {
            var residenceDecor = new ServerHousingResidenceDecor();
            for (uint i = 0u; i < quantity; i++)
            {
                Decor decor = residence.DecorCreate(entry);
                decor.Type = DecorType.Crate;

                residenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
                {
                    RealmId     = WorldServer.RealmId,
                    DecorId     = (ulong)decor.DecorId,
                    ResidenceId = residence.Id,
                    DecorType   = decor.Type,
                    PlotIndex   = decor.PlotIndex,
                    Scale       = decor.Scale,
                    Position    = decor.Position,
                    Rotation    = decor.Rotation,
                    DecorInfoId = decor.DecorInfoId
                });
            }

            EnqueueToAll(residenceDecor);
        }

        private void DecorCreate(Player player, DecorUpdate update)
        {
            HousingDecorInfoEntry entry = GameTableManager.Instance.HousingDecorInfo.GetEntry(update.DecorInfoId);
            if (entry == null)
                throw new InvalidPacketValueException();

            if (entry.CostCurrencyTypeId != 0u && entry.Cost != 0u)
            {
                /*if (!player.CurrencyManager.CanAfford((byte)entry.CostCurrencyTypeId, entry.Cost))
                {
                    // TODO: show error
                    return;
                }

                player.CurrencyManager.CurrencySubtractAmount((byte)entry.CostCurrencyTypeId, entry.Cost);*/
            }

            Decor decor = residence.DecorCreate(entry);
            decor.Type = update.DecorType;

            if (update.ColourShiftId != decor.ColourShiftId)
            {
                if (update.ColourShiftId != 0u)
                {
                    ColorShiftEntry colourEntry = GameTableManager.Instance.ColorShift.GetEntry(update.ColourShiftId);
                    if (colourEntry == null)
                        throw new InvalidPacketValueException();
                }

                decor.ColourShiftId = update.ColourShiftId;
            }

            if (update.DecorType != DecorType.Crate)
            {
                if (update.Scale < 0f)
                    throw new InvalidPacketValueException();

                // new decor is being placed directly in the world
                decor.Position = update.Position;
                decor.Rotation = update.Rotation;
                decor.Scale    = update.Scale;
            }

            EnqueueToAll(new ServerHousingResidenceDecor
            {
                Operation = 0,
                DecorData = new List<ServerHousingResidenceDecor.Decor>
                {
                    new ServerHousingResidenceDecor.Decor
                    {
                        RealmId     = WorldServer.RealmId,
                        DecorId     = (ulong)decor.DecorId,
                        ResidenceId = residence.Id,
                        DecorType   = decor.Type,
                        PlotIndex   = decor.PlotIndex,
                        Scale       = decor.Scale,
                        Position    = decor.Position,
                        Rotation    = decor.Rotation,
                        DecorInfoId = decor.DecorInfoId,
                        ColourShift = decor.ColourShiftId
                    }
                }
            });
        }

        private void DecorMove(Player player, DecorUpdate update)
        {
            Decor decor = residence.GetDecor(update.DecorId);
            if (decor == null)
                throw new InvalidPacketValueException();

            HousingResult GetResult()
            {
                if (!IsValidPlotForPosition(update))
                    return HousingResult.Decor_InvalidPosition;

                return HousingResult.Success;
            }

            HousingResult result = GetResult();
            if (result == HousingResult.Success)
            {
                if (update.PlotIndex != decor.PlotIndex)
                {
                    decor.PlotIndex = update.PlotIndex;
                }

                if (update.ColourShiftId != decor.ColourShiftId)
                {
                    if (update.ColourShiftId != 0u)
                    {
                        ColorShiftEntry colourEntry = GameTableManager.Instance.ColorShift.GetEntry(update.ColourShiftId);
                        if (colourEntry == null)
                            throw new InvalidPacketValueException();
                    }

                    decor.ColourShiftId = update.ColourShiftId;
                }

                if (decor.Type == DecorType.Crate)
                {
                    // crate->world
                    decor.Move(update.DecorType, update.Position, update.Rotation, update.Scale);
                }
                else
                {
                    if (update.DecorType == DecorType.Crate)
                        decor.Crate();
                    else
                    {
                        // world->world
                        decor.Move(update.DecorType, update.Position, update.Rotation, update.Scale);
                        decor.DecorParentId = update.ParentDecorId;
                    }
                }

                EnqueueToAll(new ServerHousingResidenceDecor
                {
                    Operation = 0,
                    DecorData = new List<ServerHousingResidenceDecor.Decor>
                    {
                        new ServerHousingResidenceDecor.Decor
                        {
                            RealmId       = WorldServer.RealmId,
                            DecorId       = (ulong)decor.DecorId,
                            ResidenceId   = residence.Id,
                            DecorType     = decor.Type,
                            PlotIndex     = decor.PlotIndex,
                            Scale         = decor.Scale,
                            Position      = decor.Position,
                            Rotation      = decor.Rotation,
                            DecorInfoId   = decor.Entry.Id,
                            ParentDecorId = decor.DecorParentId,
                            ColourShift   = decor.ColourShiftId
                        }
                    }
                });
            }
            else
            {
                player.Session.EnqueueMessageEncrypted(new ServerHousingResult
                {
                    RealmId     = WorldServer.RealmId,
                    ResidenceId = residence.Id,
                    PlayerName  = player.Name,
                    Result      = result
                });
            }
        }

        private void DecorDelete(DecorUpdate update)
        {
            Decor decor = residence.GetDecor(update.DecorId);
            if (decor == null)
                throw new InvalidPacketValueException();

            if (decor.Position != Vector3.Zero)
                throw new InvalidOperationException();

            RemoveDecorEntity(decor);
            residence.DecorDelete(decor);

            // TODO: send packet to remove from decor list
            var residenceDecor = new ServerHousingResidenceDecor();
            residenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
            {
                RealmId = WorldServer.RealmId,
                ResidenceId = residence.Id,
                DecorId = (ulong)decor.DecorId,
                DecorInfoId = 0
            });

            EnqueueToAll(residenceDecor);
        }

        /// <summary>
        /// Used to confirm the position and PlotIndex are valid together when placing Decor
        /// </summary>
        private bool IsValidPlotForPosition(DecorUpdate update)
        {
            if (update.PlotIndex == int.MaxValue)
                return true;

            WorldSocketEntry worldSocketEntry = GameTableManager.Instance.WorldSocket.GetEntry(residence.GetPlot((byte)update.PlotIndex).PlotEntry.WorldSocketId);

            // TODO: Calculate position based on individual maps on Community & Warplot residences
            Vector3 worldPosition = new Vector3(1472f + update.Position.X, update.Position.Y, 1440f + update.Position.Z);

            (uint gridX, uint gridZ) = MapGrid.GetGridCoord(worldPosition);
            (uint localCellX, uint localCellZ) = MapCell.GetCellCoord(worldPosition);
            (uint globalCellX, uint globalCellZ) = (gridX * MapDefines.GridCellCount + localCellX, gridZ * MapDefines.GridCellCount + localCellZ);

            // TODO: Investigate need for offset.
            // Offset added due to calculation being +/- 1 sometimes when placing very close to plots. They were valid placements in the client, though.
            uint maxBound = worldSocketEntry.BoundIds.Max() + 2;
            uint minBound = worldSocketEntry.BoundIds.Min() - 2;

            log.Debug($"IsValidPlotForPosition - PlotIndex: {update.PlotIndex}, Range: {minBound}-{maxBound}, Coords: {globalCellX}, {globalCellZ}");

            return (globalCellX >= minBound && globalCellX <= maxBound && globalCellZ >= minBound && globalCellZ <= maxBound);
        }

        /// <summary>
        /// Rename <see cref="Residence"/>, this is called directly from a packet hander.
        /// </summary>
        public void Rename(Player player, ClientHousingRenameProperty housingRenameProperty)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            residence.Name = housingRenameProperty.Name;
            SendHousingProperties();
        }

        /// <summary>
        /// Set <see cref="ResidencePrivacyLevel"/>, this is called directly from a packet hander.
        /// </summary>
        public void SetPrivacyLevel(Player player, ClientHousingSetPrivacyLevel housingSetPrivacyLevel)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            if (housingSetPrivacyLevel.PrivacyLevel == ResidencePrivacyLevel.Public)
                ResidenceManager.Instance.RegisterResidenceVists(residence.Id, residence.OwnerName, residence.Name);
            else
                ResidenceManager.Instance.DeregisterResidenceVists(residence.Id);

            residence.PrivacyLevel = housingSetPrivacyLevel.PrivacyLevel;
            SendHousingPrivacy();
        }

        /// <summary>
        /// Remodel <see cref="Residence"/>, this is called directly from a packet hander.
        /// </summary>
        public void Remodel(Player player, ClientHousingRemodel housingRemodel)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            if (housingRemodel.RoofDecorInfoId != 0u)
                residence.Roof = (ushort)housingRemodel.RoofDecorInfoId;
            if (housingRemodel.WallpaperId != 0u)
                residence.Wallpaper = (ushort)housingRemodel.WallpaperId;
            if (housingRemodel.EntrywayDecorInfoId != 0u)
                residence.Entryway = (ushort)housingRemodel.EntrywayDecorInfoId;
            if (housingRemodel.DoorDecorInfoId != 0u)
                residence.Door = (ushort)housingRemodel.DoorDecorInfoId;
            if (housingRemodel.SkyWallpaperId != 0u)
                residence.Sky = (ushort)housingRemodel.SkyWallpaperId;
            if (housingRemodel.MusicId != 0u)
                residence.Music = (ushort)housingRemodel.MusicId;
            if (housingRemodel.GroundWallpaperId != 0u)
                residence.Ground = (ushort)housingRemodel.GroundWallpaperId;

            SendHousingProperties();
        }

        /// <summary>
        /// UpdateResidenceFlags <see cref="Residence"/>, this is called directly from a packet hander.
        /// </summary>
        public void UpdateResidenceFlags(Player player, ClientHousingFlagsUpdate flagsUpdate)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            residence.Flags           = flagsUpdate.Flags;
            residence.ResourceSharing = flagsUpdate.ResourceSharing;
            residence.GardenSharing   = flagsUpdate.GardenSharing;

            SendHousingProperties();

            foreach (GridEntity entity in entities.Values.Where(i => i is Player))
                entity.OnRelocate(entity.Position); // Instructs entity to update their vision.
        }

        /// <summary>
        /// Used to confirm the position and PlotIndex are valid together when placing Decor
        /// </summary>
        private Vector3 CalculateWorldCoordinates(Vector3 position)
        {
            return new Vector3(1472f + position.X, -715f + position.Y, 1440f + position.Z);
        }

        /// <summary>
        /// Returns a <see cref="Vector3"/> representing local coordinates from a world coordinate.
        /// </summary>
        private Vector3 CalculateLocalCoordinates(Vector3 position)
        {
            return new Vector3(position.X - 1472f, position.Y - -715f, position.Z - 1440f);
        }

        public void DeleteDecorEntity(Player player, ClientHousingPropUpdate propRequest)
        {
            if (propRequest.PropId == 0u)
                return;

            if (propRequest.PropId == (long)residence.Id || propRequest.PropId == (long.MinValue + (long)residence.Id))
            {
                if (instancedTemporaryDecor.TryGetValue(propRequest.PropId, out TemporaryDecor temporaryDecor))
                    player.Session.EnqueueMessageEncrypted(new ServerEntityDestroy
                    {
                        Guid = temporaryDecor.Entity.Guid
                    });

                return;
            }

            Decor decor = residence.GetDecor((ulong)propRequest.PropId);
            if (decor == null)
                return; // Client asks to remove entities from old map if you switch from 1 Residence to another

            if (decor.Type == DecorType.Crate)
                return;

            if (decor.Entity == null) // TODO: Error when all entities are supported.
                return;

            player.Session.EnqueueMessageEncrypted(new ServerEntityDestroy
            {
                Guid = decor.Entity.Guid
            });
        }

        public void RequestDecorEntity(Player player, ClientHousingPropUpdate propRequest)
        {
            // Handle External and Internal 2x2 Plug Doors
            if (propRequest.PropId == (long)residence.Id || propRequest.PropId == (long.MinValue + (long)residence.Id))
            {
                if (!instancedTemporaryDecor.TryGetValue(propRequest.PropId, out TemporaryDecor temporaryDecor))
                {
                    HousingDecorInfoEntry entry = GameTableManager.Instance.HousingDecorInfo.GetEntry(propRequest.DecorId);
                    if (entry == null)
                        throw new InvalidOperationException($"HousingDecorInfoEntry {propRequest.DecorId} not found!");

                    temporaryDecor = new TemporaryDecor(Id, propRequest.PropId, entry, CalculateLocalCoordinates(propRequest.Position), propRequest.Rotation);
                    instancedTemporaryDecor.Add(temporaryDecor.DecorId, temporaryDecor);

                    InitialiseDecorEntity(temporaryDecor, propRequest.Position, propRequest.Rotation);

                    temporaryDecor.Entity.InitialiseTemporaryEntity();
                }

                SendDecorEntityRequestMessages(player, temporaryDecor);
                return;
            }

            Decor decor = residence.GetDecor((ulong)propRequest.PropId);
            if (decor == null)
                throw new InvalidOperationException();

            if (decor.Entity == null)
                CreateOrMoveDecorEntity(player, propRequest);
            
            if (decor.Entity == null) // TODO: Error when all entities are supported.
                return;

            SendDecorEntityRequestMessages(player, decor);
        }

        private void SendDecorEntityRequestMessages(Player player, Decor decor)
        {
            if (decor.Entity == null)
                return;

            player.Session.EnqueueMessageEncrypted(decor.Entity.BuildCreatePacket());
            player.Session.EnqueueMessageEncrypted(new Server08B3
            {
                MountGuid = decor.Entity.Guid,
                Unknown1 = true
            });
            player.Session.EnqueueMessageEncrypted(new Server053A
            {
                RealmId = WorldServer.RealmId,
                ResidenceId = residence.Id,
                ActivePropId = (long)decor.DecorId,
                UnitId = decor.Entity.Guid
            });

            log.Info($"Guid: {decor.Entity.Guid}");
        }

        public void CreateOrMoveDecorEntity(Player player, ClientHousingPropUpdate propRequest)
        {
            if (propRequest.PropId == (long)residence.Id || propRequest.PropId == (long.MinValue + (long)residence.Id))
                return;

            Decor propRequestDecor = residence.GetDecor((ulong)propRequest.PropId);
            if (propRequestDecor == null)
                throw new InvalidOperationException($"Decor should exist!");

            if (propRequestDecor.Type == DecorType.Crate)
                return; // TODO: Draw Entity temporarily when the Player is placing from Crate

            if (propRequestDecor.Entity != null)
            {
                if (propRequest.Position == propRequestDecor.Position && propRequest.Rotation == propRequestDecor.Rotation)
                    return;

                // TODO: Calculate entity locations instead of relying on client data
                propRequestDecor.Entity.Rotation = propRequest.Rotation.ToEulerDegrees();
                propRequestDecor.Entity.MovementManager.SetRotation(propRequest.Rotation.ToEulerDegrees());
                propRequestDecor.Entity.MovementManager.SetPosition(propRequest.Position);
                return;
            }

            InitialiseDecorEntity(propRequestDecor, propRequest.Position, propRequest.Rotation);
            if (propRequestDecor.Entity == null)
                return;

            decorEntities.TryAdd(propRequestDecor.DecorId, propRequestDecor);
        }

        private void InitialiseDecorEntity(Decor decor, Vector3 position, Quaternion rotation)
        {
            Creature2Entry entry = GetCreatureEntryForDecor(decor);
            if (entry == null)
                return;

            WorldEntity entity = CreateEntityForDecor(decor, entry, position, rotation);
            if (entity == null)
                return;

            decor.SetEntity(entity);
            entity.InitialiseTemporaryEntity();
        }

        public bool TryGetDecorEntity(uint guid, out WorldEntity entity)
        {
            entity = null;

            foreach (Decor decor in decorEntities.Values)
            {
                if (decor.Entity.Guid == guid)
                {
                    entity = decor.Entity;
                    return true;
                }
            }

            return false;
        }

        private Creature2Entry GetCreatureEntryForDecor(Decor decor, bool skipDecorCheck = false)
        {
            if (!skipDecorCheck)
            {
                Creature2Entry creatureEntry = null;
                if (decor.Entry?.Creature2IdActiveProp > 0 && decor.Entry?.Creature2IdActiveProp < uint.MaxValue)
                    creatureEntry = GameTableManager.Instance.Creature2.GetEntry(decor.Entry?.Creature2IdActiveProp ?? 0ul);

                if (creatureEntry != null)
                    return creatureEntry;
            }

            switch (decor.Entry.HousingDecorTypeId)
            {
                case 4:
                case 21:
                    return GameTableManager.Instance.Creature2.GetEntry(57195); // Generic Chair - Sitting Active Prop - Sniffs showed this was used in every seat entity.
                case 28:
                    return GameTableManager.Instance.Creature2.GetEntry(25282); // Target Dummy
                default:
                    TextTable tt = GameTableManager.Instance.GetTextTable(Language.English);
                    log.Warn($"Unsupported Decor CreatureEntry: {tt.GetEntry(GameTableManager.Instance.HousingDecorType.GetEntry(decor.Entry.HousingDecorTypeId).LocalizedTextId)}");
                    break;
            }

            return null;
        }

        private WorldEntity CreateEntityForDecor(Decor decor, Creature2Entry creatureEntry, Vector3 position, Quaternion rotation)
        {
            if (creatureEntry == null)
                throw new ArgumentNullException(nameof(creatureEntry));

            switch (creatureEntry.CreationTypeEnum)
            {
                case 0:
                    NonPlayer nonPlayerEntity = new NonPlayer(creatureEntry, decor is TemporaryDecor ? ((TemporaryDecor)decor).DecorId : (long)decor.DecorId, GetPlotId(decor.PlotIndex)); // TODO: Update PlugId to match ID of Plot at Decor's PlotIndex.
                    return SetDecorEntityProperties(decor, nonPlayerEntity, position, rotation);
                case 10:
                    Simple activateEntity = new Simple(creatureEntry, decor.ClientDecorId != 0u ? (long)decor.ClientDecorId : decor is TemporaryDecor ? ((TemporaryDecor)decor).DecorId : (long)decor.DecorId, GetPlotId(decor.PlotIndex)); // TODO: Update PlugId to match ID of Plot at Decor's PlotIndex.
                    
                    return SetDecorEntityProperties(decor, activateEntity, position, rotation);
                default:
                    log.Warn($"Unsupported Entity CreationType: {(EntityType)creatureEntry.CreationTypeEnum} ({creatureEntry.CreationTypeEnum})");
                    return null;
            }
        }

        private ushort GetPlotId(uint plotIndex)
        {
            HousingPlotInfoEntry entry = residence.GetPlot(plotIndex)?.PlotEntry;
            if (entry == null)
                return (ushort)(residence.GetPlot(plotIndex)?.PlotEntry.WorldSocketId ?? 1159);

            return (ushort)entry.WorldSocketId;
        }

        private WorldEntity SetDecorEntityProperties(Decor decor, WorldEntity entity, Vector3 position, Quaternion rotation)
        {
            entity.Rotation = rotation.ToEulerDegrees();
            entity.SetPosition(position);
            entity.IsDecorEntity = true;
            entity.SetGuid(entityCounter.Dequeue());

            //if (!(decor is TemporaryDecor) && CalculateWorldCoordinates(decor.Position) != position)
            //    log.Trace($"Positions don't match: {CalculateWorldCoordinates(decor.Position)} ~ {position}");

            //entity.OnAddToMap(this, entityCounter.Dequeue(), position);
            
            //entity.SetGuid(entityCounter.Dequeue());
            return entity;
        }

        private void RemoveDecorEntity(Decor decor)
        {
            if (decor.Entity == null)
                return;

            entityCounter.Enqueue(decor.Entity.Guid);

            if (decor is TemporaryDecor)
                instancedTemporaryDecor.Remove(((TemporaryDecor)decor).DecorId);
            else
                decorEntities.Remove(decor.DecorId);
        }
        
        /// <summary>
        /// Sends <see cref="ServerHousingPlots"/> to the player
        /// </summary>
        public void SendHousingPlots(Player player = null)
        {
            var housingPlots = new ServerHousingPlots
            {
                RealmId = WorldServer.RealmId,
                ResidenceId = residence.Id,
            };

            foreach (Plot plot in residence.GetPlots())
            {
                housingPlots.Plots.Add(new ServerHousingPlots.Plot
                {
                    PlotPropertyIndex = plot.Index,
                    PlotInfoId = plot.PlotEntry.Id,
                    PlugFacing = plot.PlugFacing,
                    PlugItemId = plot.PlugEntry?.Id ?? 0u,
                    BuildState = plot.BuildState,
                    HousingUpkeepTime = -152595.9375f,
                    BuildStartTime = plot.GetBuildStartTime()
                });
            }

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingPlots);
            else
                EnqueueToAll(housingPlots);
        }

        /// <summary>
        /// Install a House Plug into a Plot
        /// </summary>
        private void SetHousePlug(Player player, Plot plot, ClientHousingPlugUpdate housingPlugUpdate, HousingPlugItemEntry plugItemEntry)
        {
            // Update the Plot and queue necessary plug updates
            if (residence.SetHouse(plugItemEntry))
                HandleHouseChange(player, plot, housingPlugUpdate);
            else
                player.Session.EnqueueMessageEncrypted(new ServerHousingResult
                {
                    RealmId = WorldServer.RealmId,
                    ResidenceId = residence.Id,
                    PlayerName = player.Name,
                    Result = HousingResult.Plug_InvalidPlug
                });
        }

        /// <summary>
        /// Handles updating the client with changes following a 2x2 Plot change
        /// </summary>
        private void HandleHouseChange(Player player, Plot plot, ClientHousingPlugUpdate housingPlugUpdate = null)
        {
            if (housingPlugUpdate == null)
                plot.SetPlug(this, 18, player); // Defaults to Starter Tent
            else
                plot.SetPlug(this, housingPlugUpdate.PlugItem, player);

            foreach (Decor decor in residence.GetDecor().Where(d => d.Type == DecorType.InteriorDecoration))
            {
                residence.DecorDelete(decor);

                // TODO: send packet to remove from decor list
                var residenceDecor = new ServerHousingResidenceDecor();
                residenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
                {
                    RealmId = WorldServer.RealmId,
                    ResidenceId = residence.Id,
                    DecorId = (ulong)decor.DecorId,
                    DecorInfoId = 0
                });

                EnqueueToAll(residenceDecor);
            }

            foreach (Decor decor in residence.GetPlacedDecor(plot.Index).ToList())
            {
                foreach (WorldEntity entity in entities.Values.Where(i => i is Player))
                    entity.RemoveVisible(decor.Entity);

                decor.Crate();
            }

            foreach (Decor decor in decorEntities.Values.Where(d => d.Entity != null))
                foreach (WorldEntity entity in entities.Values.Where(i => i is Player))
                    entity.RemoveVisible(decor.Entity);

            foreach (TemporaryDecor decor in instancedTemporaryDecor.Values.Where(d => d.Entity != null).ToList())
            {
                foreach (WorldEntity entity in entities.Values.Where(i => i is Player))
                    entity.RemoveVisible(decor.Entity);

                RemoveDecorEntity(decor);
            }
        }

        /// <summary>
        /// Install a Plug into a Plot; Should only be called on a client update.
        /// </summary>
        public void SetPlug(Player player, ClientHousingPlugUpdate housingPlugUpdate)
        {   
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();

            Plot plot = residence.GetPlot(housingPlugUpdate.PlotInfo);
            if (plot == null)
                throw new HousingException();

            HousingPlugItemEntry plugItemEntry = GameTableManager.Instance.HousingPlugItem.GetEntry(housingPlugUpdate.PlugItem);
            if (plugItemEntry == null)
                throw new InvalidPacketValueException();

            // TODO: Confirm that this plug is usable in said slot

            // TODO: Charge the Player

            if (plot.Index == 0)
                SetHousePlug(player, plot, housingPlugUpdate, plugItemEntry);
            else
                plot.SetPlug(this, housingPlugUpdate.PlugItem, player);
        }

        /// <summary>
        /// Updates <see cref="Plot"/> to have no plug installed; Should only be called on a client update.
        /// </summary>
        public void RemovePlug(Player player, ClientHousingPlugUpdate housingPlugUpdate)
        {
            if (!residence.CanModifyResidence(player.CharacterId))
                throw new InvalidPacketValueException();
                
            Plot plot = residence.GetPlot(housingPlugUpdate.PlotInfo);
            if (plot == null)
                throw new HousingException();

            // Handle changes if plot is the house plot
            if (plot.Index == 0)
                RemoveHouse(player, plot);
            else
            {
                plot.PlugEntity.RemoveFromMap();
                plot.RemovePlug();

                SendHousingPlots();
            }
        }

        /// <summary>
        /// Updates supplied <see cref="Plot"/> to have no house on it
        /// </summary>
        private void RemoveHouse(Player player, Plot plot)
        {
            if (plot.Index > 0)
                throw new ArgumentOutOfRangeException("plot.Index", "Plot Index must be 0 to remove a house");

            // Clear all House information from the Residence instance associated with this map
            residence.RemoveHouse();

            // Even when no house is being used, the plug must be set for the house otherwise clients will get stuck on load screen
            HandleHouseChange(player, plot);

            player.Session.EnqueueMessageEncrypted(new ServerTutorial
            {
                TutorialId = 142
            });
        }

        /// <summary>
        /// Return all <see cref="GridEntity"/>'s in map that satisfy <see cref="ISearchCheck"/>.
        /// </summary>
        public override void Search(Vector3 vector, float radius, ISearchCheck check, out List<GridEntity> intersectedEntities, GridEntity searcher = null)
        {
            base.Search(vector, radius, check, out intersectedEntities);

            if (radius < 0)
            {
                if (searcher != null && searcher is Player player)
                {
                    // TODO: Add visual requirements into database or have dominion/exile phases?
                    foreach (GridEntity entity in intersectedEntities.ToList())
                    {
                        if (((WorldEntity)entity).CreatureId == 68423 && player.Faction1 != Faction.Exile) // Exile Renown Vendor
                            intersectedEntities.Remove(entity);

                        if (((WorldEntity)entity).CreatureId == 68424 && player.Faction1 != Faction.Dominion) // Dominion Renown Vendor
                            intersectedEntities.Remove(entity);

                        if (((WorldEntity)entity).CreatureId == 52542 && (player.Faction1 != Faction.Exile || (residence.Flags & ResidenceFlags.HideNeighborSkyplots) != 0)) // Exile Floating Housing Plot
                            intersectedEntities.Remove(entity);

                        if (((WorldEntity)entity).CreatureId == 52545 && (player.Faction1 != Faction.Dominion || (residence.Flags & ResidenceFlags.HideNeighborSkyplots) != 0)) // Dominion Floating Housing Plot
                            intersectedEntities.Remove(entity);
                    }
                }
                return;
            }

            //// TODO: Fix range check support

            foreach (Decor decor in decorEntities.Values.Where(d => d.Entity != null && check.CheckEntity(d.Entity)))
                intersectedEntities.Add(decor.Entity);

            foreach (Decor decor in instancedTemporaryDecor.Values.Where(d => d.Entity != null && check.CheckEntity(d.Entity)))
                intersectedEntities.Add(decor.Entity);
        }
    }
}
