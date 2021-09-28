using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Housing.Static;
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

        /// <summary>
        /// Initialise <see cref="ResidenceMapInstance"/> with <see cref="Residence"/>.
        /// </summary>
        public void Initialise(Residence residence)
        {
            AddResidence(residence);
            foreach (ResidenceChild childResidence in residence.GetChildren())
                AddResidence(childResidence.Residence);
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
            plot.SetPlugEntity(plug);
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

            SendResidences(player);
            SendResidencePlots(player);
            SendResidenceDecor(player);

            // this shows the housing toolbar, might need to move this to a more generic place in the future
            player.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet = ShortcutSet.FloatingSpellBar,
                ActionBarShortcutSetId = 1553,
                Guid = player.Guid
            });
        }

        protected override void OnUnload()
        {
            foreach (Residence residence in residences.Values.ToList())
                RemoveResidence(residence);
        }

        private void SendResidences(Player player = null)
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

        private void SendResidencePlots(Player player = null)
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

        private void SendHousingProperties(Player player = null)
        {
            List<ServerHousingProperties.Residence> residenceList = new List<ServerHousingProperties.Residence>();
            foreach(var residence in residences.Values)
            {
                residenceList.Add(new ServerHousingProperties.Residence
                {
                    RealmId = WorldServer.RealmId,
                    ResidenceId = residence.Id,
                    NeighbourhoodId = 0x190000000000000A,
                    CharacterIdOwner = residence.OwnerId,
                    Name = residence.Name,
                    PropertyInfoId = residence.PropertyInfoId,
                    ResidenceInfoId = residence.ResidenceInfoEntry?.Id ?? 0u,
                    WallpaperExterior = residence.Wallpaper,
                    Entryway = residence.Entryway,
                    Roof = residence.Roof,
                    Door = residence.Door,
                    Ground = residence.Ground,
                    Music = residence.Music,
                    Sky = residence.Sky,
                    Flags = residence.Flags,
                    ResourceSharing = residence.ResourceSharing,
                    GardenSharing = residence.GardenSharing
                });
            }
            var housingProperties = new ServerHousingProperties
            {
                Residences = residenceList
            };

            if (player != null)
                player.Session.EnqueueMessageEncrypted(housingProperties);
            else
                EnqueueToAll(housingProperties);
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
                            throw new InvalidPacketValueException();
                    }

                    decor.ColourShiftId = update.ColourShiftId;
                }

                if (decor.Type == DecorType.Crate)
                {
                    if (decor.Entry.Creature2IdActiveProp != 0u)
                    {
                        // TODO: used for decor that have an associated entity
                    }

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

            residence.DecorDelete(decor);

            // TODO: send packet to remove from decor list
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
            var worldPosition = new Vector3(1472f + update.Position.X, update.Position.Y, 1440f + update.Position.Z);

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
        }

        /// <summary>
        /// Sends <see cref="ServerHousingPlots"/> to the player
        /// </summary>
        private void SendHousingPlots(Residence residence, Player player = null)
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
                    PlotInfoId = plot.PlotInfoEntry.Id,
                    PlugFacing = plot.PlugFacing,
                    PlugItemId = plot.PlugItemEntry?.Id ?? 0u,
                    BuildState = plot.BuildState
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
        private void SetHousePlug(Residence residence, Player player, ClientHousingPlugUpdate housingPlugUpdate, HousingPlugItemEntry plugItemEntry)
        {
            if (!residence.CanModifyResidence(player))
                throw new InvalidPacketValueException();

            Plot plot = residence.GetPlot(housingPlugUpdate.PlotInfo);
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
            SendHousingProperties();

            // Send plots again after Property was updated
            SendHousingPlots(residence);

            // Resend the Action Bar because buttons may've been enabled after adding a house
            player.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet = ShortcutSet.FloatingSpellBar,
                ActionBarShortcutSetId = 1553,
                Guid = player.Guid
            });

            // Send plots again after Property was updated
            SendHousingPlots(residence);

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

            Plot plot = residence.GetPlot(housingPlugUpdate.PlotInfo);
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

                SendHousingPlots(residence);

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

            SendHousingPlots(residence);

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

            Plot plot = residence.GetPlot(housingPlugUpdate.PlotInfo);
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

                SendHousingPlots(residence);
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
    }
}
