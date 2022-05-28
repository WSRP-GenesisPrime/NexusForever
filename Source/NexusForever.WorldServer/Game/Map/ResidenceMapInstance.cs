using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Shared;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.GameTable.Static;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Housing.Static;
using NexusForever.WorldServer.Game.Map.Search;
using NexusForever.WorldServer.Game.Reputation.Static;
using NexusForever.WorldServer.Game.Spell.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;

namespace NexusForever.WorldServer.Game.Map
{
    public class ResidenceMapInstance : MapInstance
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        // housing maps have unlimited vision range.
        public override float VisionRange { get; protected set; } = -1f;

        private readonly Dictionary<ulong, Residence> residences = new();

        private readonly Dictionary</* decorId */ long, Decor> decorEntities = new Dictionary<long, Decor>();

        /// <summary>
        /// Initialise <see cref="ResidenceMapInstance"/> with <see cref="Residence"/>.
        /// </summary>
        public void Initialise(Residence residence)
        {
            AddResidence(residence);
            foreach (ResidenceChild childResidence in residence.GetChildren())
                AddResidence(childResidence.Residence);
        }
        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            foreach (Decor decor in decorEntities.Values)
                decor.Entity?.Update(lastTick);
        }

        private void AddResidence(Residence residence)
        {
            residences.Add(residence.Id, residence);
            residence.Map = this;

            foreach (Plot plot in residence.GetPlots()
                .Where(p => p.PlugItemEntry != null))
                AddPlugEntity(plot);
        }

        private void AddPlugEntity(Plot plot)
        {
            var plug = new Plug(plot.PlotInfoEntry, plot.PlugItemEntry);
            plot.PlugEntity = plug;

            EnqueueAdd(plug, new MapPosition
            {
                Position = Vector3.Zero
            });
        }

        public Residence GetResidenceByPropertyInfoId(PropertyInfoId slot)
        {
            return residences.Where(r => r.Value.PropertyInfoId == slot).FirstOrDefault().Value;
        }

        private static readonly Dictionary<uint, PropertyInfoId> ZoneToPropertyInfo = new Dictionary<uint, PropertyInfoId>()
        {
            { 1136, PropertyInfoId.Residence },
            { 1265, PropertyInfoId.Community },
            { 1161, PropertyInfoId.CommunityResidence1 },
            { 1162, PropertyInfoId.CommunityResidence2 },
            { 1163, PropertyInfoId.CommunityResidence3 },
            { 1164, PropertyInfoId.CommunityResidence4 },
            { 1165, PropertyInfoId.CommunityResidence5 },
        };

        public Residence GetResidenceByZone(WorldZoneEntry zone)
        {
            if (ZoneToPropertyInfo.TryGetValue(zone.Id, out PropertyInfoId id))
            {
                return GetResidenceByPropertyInfoId(id);
            }
            return null;
        }

        private void RemoveResidence(Residence residence)
        {
            residences.Remove(residence.Id);
            residence.Map = null;

            foreach (Plot plot in residence.GetPlots()
                .Where(p => p.PlugItemEntry != null))
                plot?.PlugEntity.RemoveFromMap();
        }

        protected override MapPosition GetPlayerReturnLocation(Player player)
        {
            // if the residence is unloaded return player to their own residence
            Residence returnResidence = GlobalResidenceManager.Instance.GetResidenceByOwner(player.Name);
            returnResidence ??= GlobalResidenceManager.Instance.CreateResidence(player);
            ResidenceEntrance entrance = GlobalResidenceManager.Instance.GetResidenceEntrance(returnResidence.PropertyInfoId);

            return new MapPosition
            {
                Info   = new MapInfo
                {
                    Entry      = entrance.Entry,
                    InstanceId = returnResidence.Id
                },
                Position = entrance.Position
            };
        }

        protected override void AddEntity(GridEntity entity, Vector3 vector)
        {
            base.AddEntity(entity, vector);
            if (entity is not Player player)
                return;

            if (player.SpellManager.GetSpell(25520) == null)
                player.SpellManager.AddSpell(25520);

            if (player.SpellManager.GetSpell(22919) == null)
                player.SpellManager.AddSpell(22919);

            SendResidences(player);
            SendResidencePlots(player);
            SendResidenceDecor(player);
            SendActionBars(player);

            foreach (Residence residence in residences.Values)
            {
                foreach (Decor decor in residence.GetPlacedDecor())
                    CreateEntityForDecor(residence, decor);
            }
        }

        public override void OnRemoveFromMap(Player player)
        {
            foreach (Decor decor in decorEntities.Values.Where(d => d.Entity != null))
                player.RemoveVisible(decor.Entity);
        }

        protected override void OnUnload()
        {
            foreach (Residence residence in residences.Values.ToList())
                RemoveResidence(residence);
        }

        public void SendActionBars(Player player)
        {
            // this shows the housing toolbar, might need to move this to a more generic place in the future
            player.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet = ShortcutSet.FloatingSpellBar,
                ActionBarShortcutSetId = 1553,
                Guid = player.Guid
            });
        }

        public void SendResidences(Player player = null)
        {
            var housingProperties = new ServerHousingProperties();
            foreach (Residence residence in residences.Values)
                housingProperties.Residences.Add(residence.Build());

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingProperties);
            else
                EnqueueToAll(housingProperties);
        }

        private void SendResidence(Residence residence, Player player = null)
        {
            var housingProperties = new ServerHousingProperties();
            housingProperties.Residences.Add(residence.Build());

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingProperties);
            else
                EnqueueToAll(housingProperties);
        }

        private void SendResidenceRemoved(Residence residence, Player player = null)
        {
            var housingProperties = new ServerHousingProperties();

            ServerHousingProperties.Residence residenceInfo = residence.Build();
            residenceInfo.ResidenceDeleted = true;
            housingProperties.Residences.Add(residenceInfo);

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingProperties);
            else
                EnqueueToAll(housingProperties);
        }

        public void SendResidencePlots(Player player = null)
        {
            foreach (Residence residence in residences.Values)
                SendResidencePlots(residence, player);
        }

        private void SendResidencePlots(Residence residence, Player player = null)
        {
            var housingPlots = new ServerHousingPlots
            {
                RealmId     = WorldServer.RealmId,
                ResidenceId = residence.Id
            };

            foreach (Plot plot in residence.GetPlots())
            {
                housingPlots.Plots.Add(new ServerHousingPlots.Plot
                {
                    PlotPropertyIndex = plot.Index,
                    PlotInfoId        = plot.PlotInfoEntry.Id,
                    PlugFacing        = plot.PlugFacing,
                    PlugItemId        = plot.PlugItemEntry?.Id ?? 0u,
                    BuildState        = plot.BuildState
                });
            }

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingPlots);
            else
                EnqueueToAll(housingPlots);
        }

        private void SendResidenceDecor(Player player = null)
        {
            // a separate ServerHousingResidenceDecor has to be used for each residence
            // the client uses the residence id from the first decor it receives as the storage for the rest as well
            // no idea why it was implemented like this...
            foreach (Residence residence in residences.Values)
                SendResidenceDecor(residence, player);
        }

        private void SendResidenceDecor(Residence residence, Player player = null)
        {
            var residenceDecor = new ServerHousingResidenceDecor
            {
                Operation = 0
            };

            Decor[] decors = residence.GetDecor().ToArray();
            for (uint i = 0u; i < decors.Length; i++)
            {
                Decor decor = decors[i];
                residenceDecor.DecorData.Add(decor.Build());

                // client freaks out if too much decor is sent in a single message, limit to 100
                if (i == decors.Length - 1 || i != 0u && i % 100u == 0u)
                {
                    if (player != null)
                        player.Session.EnqueueMessageEncrypted(residenceDecor);
                    else
                        EnqueueToAll(residenceDecor);

                    residenceDecor.DecorData.Clear();
                }
            }
        }

        public Residence GetMainResidence()
        {
            Residence ret = null;
            foreach (Residence residence in residences.Values)
            {
                ret = residence;
                if(residence.IsCommunityResidence)
                {
                    return residence;
                }
            }
            return ret;
        }

        /// <summary>
        /// Add child <see cref="Residence"/> to parent <see cref="Residence"/>.
        /// </summary>
        public void AddChild(Residence residence, bool temporary)
        {
            Residence community = residences.Values.SingleOrDefault(r => r.IsCommunityResidence);
            if (community == null)
                throw new InvalidOperationException("Can't add child residence to a map that isn't a community!");

            community.AddChild(residence, temporary);
            AddResidence(residence);

            SendResidence(residence);
            SendResidencePlots(residence);
            SendResidenceDecor(residence);
        }

        /// <summary>
        /// Remove child <see cref="Residence"/> to parent <see cref="Residence"/>.
        /// </summary>
        public void RemoveChild(Residence residence)
        {
            Residence community = residences.Values.SingleOrDefault(r => r.IsCommunityResidence);
            if (community == null)
                throw new InvalidOperationException("Can't remove child residence from a map that isn't a community!");

            community.RemoveChild(residence);
            RemoveResidence(residence);

            SendResidenceRemoved(residence);
        }

        /// <summary>
        /// Crate all placed <see cref="Decor"/>, this is called directly from a packet handler.
        /// </summary>
        public void CrateAllDecor(TargetResidence targetResidence, Player player)
        {
            if (!residences.TryGetValue(targetResidence.ResidenceId, out Residence residence)
                || !residence.CanModifyResidence(player))
                throw new InvalidPacketValueException();

            var housingResidenceDecor = new ServerHousingResidenceDecor();
            foreach (Decor decor in residence.GetPlacedDecor())
            {
                decor.Crate();
                housingResidenceDecor.DecorData.Add(decor.Build());

                if (decor.Entity != null)
                    decor.RemoveEntity();
            }

            EnqueueToAll(housingResidenceDecor);
        }

        /// <summary>
        /// Update <see cref="Decor"/> (create, move or delete), this is called directly from a packet handler.
        /// </summary>
        public void DecorUpdate(Player player, ClientHousingDecorUpdate housingDecorUpdate)
        {
            foreach (DecorInfo update in housingDecorUpdate.DecorUpdates)
            {
                if (!residences.TryGetValue(update.TargetResidence.ResidenceId, out Residence residence)
                    || !residence.CanModifyResidence(player))
                    throw new InvalidPacketValueException();

                switch (housingDecorUpdate.Operation)
                {
                    case DecorUpdateOperation.Create:
                        DecorCreate(residence, player, update);
                        break;
                    case DecorUpdateOperation.Move:
                        DecorMove(residence, player, update);
                        break;
                    case DecorUpdateOperation.Delete:
                        DecorDelete(residence, update);
                        break;
                    default:
                        throw new InvalidPacketValueException();
                }
            }
        }

        /// <summary>
        /// Create and add <see cref="Decor"/> from supplied <see cref="HousingDecorInfoEntry"/> to your crate.
        /// </summary>
        public void DecorCreate(Residence residence, HousingDecorInfoEntry entry, uint quantity)
        {
            var residenceDecor = new ServerHousingResidenceDecor();
            for (uint i = 0u; i < quantity; i++)
            {
                Decor decor = residence.DecorCreate(entry);
                residenceDecor.DecorData.Add(decor.Build());
            }

            EnqueueToAll(residenceDecor);
        }

        private void DecorCreate(Residence residence, Player player, DecorInfo update)
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
            decor.PlotIndex = update.PlotIndex;

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

                CreateEntityForDecor(residence, decor);
            }

            EnqueueToAll(new ServerHousingResidenceDecor
            {
                Operation = 0,
                DecorData = new List<ServerHousingResidenceDecor.Decor>
                {
                     decor.Build()
                }
            });
        }

        private void DecorMove(Residence residence, Player player, DecorInfo update)
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
                        {
                            log.Error($"ColorShiftEntry null for update.ColourShiftId: {update.ColourShiftId}; actor name: {player.Name}, residence: {residence.Id} ({residence.Name})");
                            throw new InvalidPacketValueException();
                        }
                            
                    }

                    decor.ColourShiftId = update.ColourShiftId;
                }

                if (decor.Type == DecorType.Crate)
                {
                    // crate->world
                    decor.Move(update.DecorType, update.Position, update.Rotation, update.Scale, update.PlotIndex);
                    CreateEntityForDecor(residence, decor);
                }
                else
                {
                    if (update.DecorType == DecorType.Crate)
                        decor.Crate();
                    else
                    {
                        // world->world
                        decor.Move(update.DecorType, update.Position, update.Rotation, update.Scale, update.PlotIndex);
                        decor.DecorParentId = update.ParentDecorId;
                        decor.RemoveEntity();
                        CreateEntityForDecor(residence, decor);
                    }
                }
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

            EnqueueToAll(new ServerHousingResidenceDecor
            {
                Operation = 0,
                DecorData = new List<ServerHousingResidenceDecor.Decor>
                {
                    decor.Build()
                }
            });
        }

        private void DecorDelete(Residence residence, DecorInfo update)
        {
            Decor decor = residence.GetDecor(update.DecorId);
            if (decor == null)
                throw new InvalidPacketValueException();

            if (decor.Position != Vector3.Zero)
                throw new InvalidOperationException();

            DecorDelete(residence, decor);
        }

        /// <summary>
        /// Remove an existing <see cref="Decor"/> from <see cref="Residence"/>.
        /// </summary>
        public void DecorDelete(Residence residence, Decor decor)
        {
            if (decor.PendingCreate)
                residence.DecorRemove(decor);
            else
                decor.EnqueueDelete();

            var residenceDecor = new ServerHousingResidenceDecor();
            residenceDecor.DecorData.Add(new ServerHousingResidenceDecor.Decor
            {
                RealmId     = WorldServer.RealmId,
                ResidenceId = residence.Id,
                DecorId     = decor.DecorId,
                DecorInfoId = 0
            });

            EnqueueToAll(residenceDecor);
        }

        /// <summary>
        /// Create a new <see cref="Decor"/> from an existing <see cref="Decor"/> for <see cref="Residence"/>.
        /// </summary>
        /// <remarks>
        /// Copies all data from the source <see cref="Decor"/> with a new id.
        /// </remarks>
        public void DecorCopy(Residence residence, Decor decor)
        {
            Decor newDecor = residence.DecorCopy(decor);

            var residenceDecor = new ServerHousingResidenceDecor();
            residenceDecor.DecorData.Add(newDecor.Build());
            EnqueueToAll(residenceDecor);
        }

        /// <summary>
        /// Used to confirm the position and PlotIndex are valid together when placing Decor
        /// </summary>
        private bool IsValidPlotForPosition(DecorInfo update)
        {
            return true;

            /*if (update.PlotIndex == int.MaxValue)
                return true;

            return true; // ignore boundaries, YOLO.

            /*WorldSocketEntry worldSocketEntry = GameTableManager.Instance.WorldSocket.GetEntry(residence.GetPlot((byte)update.PlotIndex).PlotEntry.WorldSocketId);

            // TODO: Calculate position based on individual maps on Community & Warplot residences
            var worldPosition = new Vector3(1472f + update.Position.Z, -715f + update.Position.Y, 1440f + update.Position.X);

            (uint gridX, uint gridZ) = MapGrid.GetGridCoord(worldPosition);
            (uint localCellX, uint localCellZ) = MapCell.GetCellCoord(worldPosition);
            (uint globalCellX, uint globalCellZ) = (gridX * MapDefines.GridCellCount + localCellX, gridZ * MapDefines.GridCellCount + localCellZ);

            // TODO: Investigate need for offset.
            // Offset added due to calculation being +/- 1 sometimes when placing very close to plots. They were valid placements in the client, though.
            uint maxBound = worldSocketEntry.BoundIds.Max() + 2;
            uint minBound = worldSocketEntry.BoundIds.Min() - 2;

            log.Debug($"IsValidPlotForPosition - PlotIndex: {update.PlotIndex}, Range: {minBound}-{maxBound}, Coords: {globalCellX}, {globalCellZ}");

            return (globalCellX >= minBound && globalCellX <= maxBound && globalCellZ >= minBound && globalCellZ <= maxBound);*/
        }

        /// <summary>
        /// Rename <see cref="Residence"/> with supplied name.
        /// </summary>
        public void RenameResidence(Player player, TargetResidence targetResidence, string name)
        {
            if (!residences.TryGetValue(targetResidence.ResidenceId, out Residence residence)
                || !residence.CanModifyResidence(player))
                throw new InvalidPacketValueException();

            RenameResidence(residence, name);
        }

        /// <summary>
        /// Rename <see cref="Residence"/> with supplied name.
        /// </summary>
        public void RenameResidence(Residence residence, string name)
        {
            residence.Name = name;
            SendResidence(residence);
        }

        /// <summary>
        /// Remodel <see cref="Residence"/>, this is called directly from a packet handler.
        /// </summary>
        public void Remodel(TargetResidence targetResidence, Player player, ClientHousingRemodel housingRemodel)
        {
            if (!residences.TryGetValue(targetResidence.ResidenceId, out Residence residence)
                || !residence.CanModifyResidence(player))
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

            SendResidences();
        }

        /// <summary>
        /// UpdateResidenceFlags <see cref="Residence"/>, this is called directly from a packet handler.
        /// </summary>
        public void UpdateResidenceFlags(TargetResidence targetResidence, Player player, ClientHousingFlagsUpdate flagsUpdate)
        {
            if (!residences.TryGetValue(targetResidence.ResidenceId, out Residence residence)
                || !residence.CanModifyResidence(player))
                throw new InvalidPacketValueException();

            residence.Flags           = flagsUpdate.Flags;
            residence.ResourceSharing = flagsUpdate.ResourceSharing;
            residence.GardenSharing   = flagsUpdate.GardenSharing;

            SendResidences();

            foreach (GridEntity entity in entities.Values.Where(i => i is Player))
                entity.OnRelocate(entity.Position); // Instructs entity to update their vision.
        }

        /// <summary>
        /// Install a House Plug into a Plot
        /// </summary>
        private void SetHousePlug(Residence residence, Player player, ClientHousingPlugUpdate housingPlugUpdate, HousingPlugItemEntry plugItemEntry)
        {
            if (!residence.CanModifyResidence(player))
                throw new InvalidPacketValueException();

            Plot plot = residence.GetPlotByPlotInfo(housingPlugUpdate.PlotInfo);
            if (plot == null)
                throw new HousingException();

            // TODO: Confirm that this plug is usable in said slot

            // TODO: Figure out how the "Construction Yard" shows up. Appears to be related to time and not a specific packet. 
            //       Telling the client that the Plots were updated looks to be the only trigger for the building animation.

            // Update the Plot and queue necessary plug updates
            if (residence.SetHouse(plugItemEntry))
            {
                HandleHouseChange(residence, player, plot, housingPlugUpdate);
            }
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
        public void HandleHouseChange(Residence residence, Player player, Plot plot, ClientHousingPlugUpdate housingPlugUpdate = null)
        {
            Vector3 houseLocation = new Vector3(1471f, -715f, 1443f);
            // TODO: Crate all decor stored inside the house?

            UpdatePlot(residence, player, plot, housingPlugUpdate, houseLocation);

            // Send updated Property data (informs residence, roof, door, etc.)
            SendResidences();

            // Send plots again after Property was updated
            SendResidencePlots(residence);

            // Resend the Action Bar because buttons may've been enabled after adding a house
            player.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet = ShortcutSet.FloatingSpellBar,
                ActionBarShortcutSetId = 1553,
                Guid = player.Guid
            });

            // Send plots again after Property was updated
            SendResidencePlots(residence);

            // Send residence decor after house change
            SendResidenceDecor();

            // TODO: Run script(s) associated with PlugItem

            // Set plot to "Built", and inform all clients
            plot.BuildState = 4;
            // TODO: Move this to an Update method, and ensure any timers are honoured before build is completed
            EnqueueToAll(new ServerHousingPlotUpdate
            {
                RealmId = WorldServer.RealmId,
                ResidenceId = residence.Id,
                PlotIndex = plot.Index,
                BuildStage = 0,
                BuildState = plot.BuildState
            });
        }

        /// <summary>
        /// Install a Plug into a Plot; Should only be called on a client update.
        /// </summary>
        public void SetPlug(Residence residence, Player player, ClientHousingPlugUpdate housingPlugUpdate)
        {
            if (!residence.CanModifyResidence(player))
                throw new InvalidPacketValueException();

            Plot plot = residence.GetPlotByPlotInfo(housingPlugUpdate.PlotInfo);
            if (plot == null)
                throw new HousingException();

            HousingPlugItemEntry plugItemEntry = GameTableManager.Instance.HousingPlugItem.GetEntry(housingPlugUpdate.PlugItem);
            if (plugItemEntry == null)
                throw new InvalidPacketValueException();

            // TODO: Confirm that this plug is usable in said slot

            if (plot.Index == 0)
                SetHousePlug(residence, player, housingPlugUpdate, plugItemEntry);
            else
            {
                // TODO: Figure out how the "Construction Yard" shows up. Appears to be related to time and not a specific packet. 
                //       Telling the client that the Plots were updated looks to be the only trigger for the building animation.

                // Update the Plot and queue necessary plug updates
                UpdatePlot(residence, player, plot, housingPlugUpdate);

                SendResidencePlots(residence);

                // TODO: Run script(s) associated with PlugItem

                // Set plot to "Built", and inform all clients
                plot.BuildState = 4;
                // TODO: Move this to an Update method, and ensure any timers are honoured before build is completed
                EnqueueToAll(new ServerHousingPlotUpdate
                {
                    RealmId = WorldServer.RealmId,
                    ResidenceId = residence.Id,
                    PlotIndex = plot.Index,
                    BuildStage = 0,
                    BuildState = plot.BuildState
                });
            }

            // TODO: Deduct any cost and/or items
        }

        /// <summary>
        /// Update supplied <see cref="Plot"/>; Should only be called by <see cref="ResidenceMap"/> when updating plot information
        /// </summary>
        private void UpdatePlot(Residence residence, Player player, Plot plot, ClientHousingPlugUpdate housingPlugUpdate = null, Vector3 position = new Vector3())
        {
            if(residence == null)
            {
                if(housingPlugUpdate == null)
                {
                    throw new ArgumentException("UpdatePlot without residence nor housingPlugUpdate");
                }
                residence = GlobalResidenceManager.Instance.GetResidence(housingPlugUpdate.ResidenceId);
            }
            // Set this Plot to use the correct Plug
            if (housingPlugUpdate != null)
                plot.SetPlug(housingPlugUpdate.PlugItem, (HousingPlugFacing)housingPlugUpdate.PlugFacing == HousingPlugFacing.Default ? HousingPlugFacing.East : (HousingPlugFacing)housingPlugUpdate.PlugFacing);

            SendResidencePlots(residence);

            // TODO: Figure out what this packet is for
            player.Session.EnqueueMessageEncrypted(new Server051F
            {
                RealmId = WorldServer.RealmId,
                ResidenceId = residence.Id,
                PlotIndex = plot.Index
            });

            // Instatiate new plug entity and assign to Plot's PlugEntity cache
            var newPlug = new Plug(plot.PlotInfoEntry, plot.PlugItemEntry);

            if (plot.PlugEntity != null)
                plot.PlugEntity.EnqueueReplace(newPlug);
            else
                EnqueueAdd(newPlug, new MapPosition {
                    Position = position
                });

            // Update plot with PlugEntity reference
            plot.SetPlugEntity(newPlug);
        }

        /// <summary>
        /// Updates <see cref="Plot"/> to have no plug installed; Should only be called on a client update.
        /// </summary>
        public void RemovePlug(Residence residence, Player player, ClientHousingPlugUpdate housingPlugUpdate)
        {
            if (!residence.CanModifyResidence(player))
                throw new InvalidPacketValueException();

            Plot plot = residence.GetPlotByPlotInfo(housingPlugUpdate.PlotInfo);
            if (plot == null)
                throw new HousingException();

            RemovePlug(residence, player, plot);
        }

        /// <summary>
        /// Updates <see cref="Plot"/> to have no plug installed
        /// </summary>
        private void RemovePlug(Residence residence, Player player, Plot plot)
        {
            // Handle changes if plot is the house plot
            if (plot.Index == 0)
                RemoveHouse(residence, player, plot);
            else
            {
                plot.PlugEntity.RemoveFromMap();
                plot.RemovePlug();

                SendResidencePlots(residence);
            }
        }

        /// <summary>
        /// Updates supplied <see cref="Plot"/> to have no house on it
        /// </summary>
        private void RemoveHouse(Residence residence, Player player, Plot plot)
        {
            if (plot.Index > 0)
                throw new ArgumentOutOfRangeException("plot.Index", "Plot Index must be 0 to remove a house");

            // Clear all House information from the Residence instance associated with this map
            residence.RemoveHouse();

            // Even when no house is being used, the plug must be set for the house otherwise clients will get stuck on load screen
            plot.SetPlug(531); // Defaults to Starter Tent

            HandleHouseChange(residence, player, plot);

            player.Session.EnqueueMessageEncrypted(new ServerTutorial
            {
                TutorialId = 142
            });
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
            Residence residence = GlobalResidenceManager.Instance.GetResidence(propRequest.ResidenceId);
            if (propRequest.PropId == 0u)
                return;

            Decor decor = residence.GetDecor(propRequest.PropId);
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
            Residence residence = GlobalResidenceManager.Instance.GetResidence(propRequest.ResidenceId);

            Decor decor = residence.GetDecor(propRequest.PropId);
            if (decor == null)
                throw new InvalidOperationException();

            if (decor.Entity == null)
                CreateOrMoveDecorEntity(player, propRequest);

            if (decor.Entity == null) // TODO: Error when all entities are supported.
                return;

            SendDecorEntityRequestMessages(residence, player, decor);
        }

        private void SendDecorEntityRequestMessages(Residence residence, Player player, Decor decor)
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

            //log.Info($"Guid: {decor.Entity.Guid}");
        }

        public void CreateOrMoveDecorEntity(Player player, ClientHousingPropUpdate propRequest)
        {
            Residence residence = GlobalResidenceManager.Instance.GetResidence(propRequest.ResidenceId);
            if (propRequest.PropId == (long)residence.Id || propRequest.PropId == (long.MinValue + (long)residence.Id))
                return;

            Decor propRequestDecor = residence.GetDecor(propRequest.PropId);
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
                if (propRequestDecor.Entity.MovementManager != null) // happens sometimes, apparently.
                {
                    propRequestDecor.Entity.MovementManager.SetRotation(propRequest.Rotation.ToEulerDegrees());
                    propRequestDecor.Entity.MovementManager.SetPosition(propRequest.Position);
                }
                return;
            }

            InitialiseDecorEntity(residence, propRequestDecor, propRequest.Position, propRequest.Rotation);
            if (propRequestDecor.Entity == null)
                return;

            decorEntities.TryAdd(propRequestDecor.DecorId, propRequestDecor);
        }

        private void InitialiseDecorEntity(Residence residence, Decor decor, Vector3 position, Quaternion rotation)
        {
            Creature2Entry entry = GetCreatureEntryForDecor(decor);
            if (entry == null)
                return;

            WorldEntity entity = CreateEntityForDecor(residence, decor, entry, position, rotation);
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

        private Creature2Entry GetCreatureEntryForDecor(Decor decor)
        {
            Creature2Entry creatureEntry = null;
            if (decor.Entry?.Creature2IdActiveProp > 0 && decor.Entry?.Creature2IdActiveProp < uint.MaxValue)
                creatureEntry = GameTableManager.Instance.Creature2.GetEntry(decor.Entry?.Creature2IdActiveProp ?? 0ul);

            if (creatureEntry != null)
                return creatureEntry;

            /*if(decor.Entry.Id >= 3699)
            {
                // Added by spreadsheet, creature2Id should be assumed correct.
                return null;
            }*/

            switch (decor.Entry.HousingDecorTypeId)
            {
                case 1:
                case 59:
                    return GameTableManager.Instance.Creature2.GetEntry(70052);
                case 4:
                case 21:
                    return GameTableManager.Instance.Creature2.GetEntry(57195); // Generic Chair - Sitting Active Prop - Sniffs showed this was used in every seat entity.
                case 28:
                    return GameTableManager.Instance.Creature2.GetEntry(25282); // Target Dummy
                default:
                    TextTable tt = GameTableManager.Instance.GetTextTable(Language.English);
                    //log.Warn($"Unsupported Decor CreatureEntry: {tt.GetEntry(GameTableManager.Instance.HousingDecorType.GetEntry(decor.Entry.HousingDecorTypeId).LocalizedTextId)}");
                    break;
            }

            return null;
        }

        private WorldEntity CreateEntityForDecor(Residence residence, Decor decor, Creature2Entry creatureEntry, Vector3 position, Quaternion rotation)
        {
            if (creatureEntry == null)
                throw new ArgumentNullException(nameof(creatureEntry));

            switch (creatureEntry.CreationTypeEnum)
            {
                case 0:
                    NonPlayer nonPlayerEntity = new NonPlayer(creatureEntry, decor.DecorId, GetPlotId(residence, decor.PlotIndex));
                    return SetDecorEntityProperties(decor, nonPlayerEntity, position, rotation);
                case 10:
                    Simple activateEntity = new Simple(creatureEntry, decor.DecorId, GetPlotId(residence, decor.PlotIndex));

                    return SetDecorEntityProperties(decor, activateEntity, position, rotation);
                default:
                    log.Warn($"Unsupported Entity CreationType: {(EntityType)creatureEntry.CreationTypeEnum} ({creatureEntry.CreationTypeEnum})");
                    return null;
            }
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

        private void CreateEntityForDecor(Residence residence, Decor decor)
        {
            if (decor.Position == Vector3.Zero)
                return;

            Creature2Entry creatureEntry = GetCreatureEntryForDecor(decor);

            if (creatureEntry != null)
                CreateEntityForDecor(residence, decor, creatureEntry, decor.Position, decor.Rotation);
        }

        private ushort GetPlotId(Residence residence, uint plotIndex)
        {
            HousingPlotInfoEntry entry = residence.GetPlotByIndex(plotIndex)?.PlotInfoEntry;
            if (entry == null)
                return (ushort)(residence.GetPlotByPlotInfo(plotIndex)?.PlotInfoEntry.WorldSocketId ?? 1159);

            return (ushort)entry.WorldSocketId;
        }

        private void RemoveDecorEntity(Decor decor)
        {
            if (decor.Entity == null)
                return;

            entityCounter.Enqueue(decor.Entity.Guid);

            decorEntities.Remove(decor.DecorId);
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
                    Residence residence = GetMainResidence();

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
        }
    }
}
