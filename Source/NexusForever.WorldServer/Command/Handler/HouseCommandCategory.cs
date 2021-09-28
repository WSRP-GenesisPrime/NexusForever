using System;
using System.Text;
using NexusForever.Shared;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Housing.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.House, "A collection of commands to modify housing residences.", "house")]
    [CommandTarget(typeof(Player))]
    public class HouseCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.HouseDecorAdd, "Add decor to housing residence crate optionally specifying quantity.", "decoradd")]
        public void HandleHouseDecorAdd(ICommandContext context,
            [Parameter("Decor info id entry to add to the crate.")]
            uint decorInfoId,
            [Parameter("Quantity of decor to add to the crate.")]
            uint? quantity)
        {
            try
            {
                quantity ??= 1u;

                HousingDecorInfoEntry entry = GameTableManager.Instance.HousingDecorInfo.GetEntry(decorInfoId);
                if (entry == null)
                {
                    context.SendMessage($"Invalid decor info id {decorInfoId}!");
                    return;
                }
                log.Info($"{context.InvokingPlayer.Name} requesting to add decor ID {decorInfoId} (x{quantity}).");
                context.GetTargetOrInvoker<Player>().ResidenceManager.DecorCreate(entry, quantity.Value);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in HouseCommandCategory.HandleHouseDecorAdd!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.HouseDecorLookup, "Returns a list of decor ids that match the supplied name.", "decorlookup")]
        public void HandleHouseDecorLookup(ICommandContext context,
            [Parameter("Name or partial name of the housing decor item to search for.")]
            string name)
        {
            try
            {
                var sw = new StringBuilder();
                sw.AppendLine("Decor Lookup Results:");

                TextTable tt = GameTableManager.Instance.GetTextTable(context.Language);
                foreach (HousingDecorInfoEntry decorEntry in
                    SearchManager.Instance.Search<HousingDecorInfoEntry>(name, context.Language, e => e.LocalizedTextIdName, true))
                {
                    string text = tt.GetEntry(decorEntry.LocalizedTextIdName);
                    sw.AppendLine($"({decorEntry.Id}) {text}");
                }

                context.SendMessage(sw.ToString());
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in HouseCommandCategory.HandleHouseDecorLookup!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.HouseTeleport, "Teleport to a residence, optionally specifying a character.", "teleport")]
        public void HandleHouseTeleport(ICommandContext context,
            [Parameter("", ParameterFlags.Optional)]
            string firstName,
            [Parameter("", ParameterFlags.Optional)]
            string lastName)
        {
            try
            {
                Player target = context.InvokingPlayer;
                if (!target.CanTeleport())
                {
                    context.SendMessage("You have a pending teleport! Please wait to use this command.");
                    return;
                }

                string name = $"{firstName} {lastName}";
                if (firstName == null && lastName == null)
                {
                    name = target.Name;
                }

                log.Info($"{target.Name} requesting teleport to plot {name}.");

                Residence residence = GlobalResidenceManager.Instance.GetResidenceByOwner(name);
                if (residence == null)
                {
                    if (firstName == null && lastName == null)
                    {
                        residence = GlobalResidenceManager.Instance.CreateResidence(target);
                        log.Info($"Creating residence {residence.Id} for name {name}, firstname {firstName}, lastname {lastName}.");
                    }
                    else
                    {
                        context.SendMessage("A residence for that character doesn't exist!");
                        return;
                    }
                }

                if (residence.OwnerId != context.InvokingPlayer.CharacterId)
                {
                    if (residence.Has18PlusLock())
                    {
                        if (!context.InvokingPlayer.IsAdult)
                        {
                            context.InvokingPlayer.SendSystemMessage("This plot is currently unavailable.");
                            return;
                        }
                    }

                    switch (residence.PrivacyLevel)
                    {
                        case ResidencePrivacyLevel.Private:
                            {
                                context.InvokingPlayer.SendSystemMessage("This plot is currently unavailable.");
                                return;
                            }
                        // TODO: check if player is either a neighbour or roommate
                        case ResidencePrivacyLevel.NeighborsOnly:
                            break;
                        case ResidencePrivacyLevel.RoommatesOnly:
                            break;
                    }
                }

                ResidenceEntrance entrance = GlobalResidenceManager.Instance.GetResidenceEntrance(residence.PropertyInfoId);
                target.Rotation = entrance.Rotation.ToEulerDegrees();
                target.TeleportTo(entrance.Entry, entrance.Position, residence.Parent?.Id ?? residence.Id);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in HouseCommandCategory.HandleHouseTeleport!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.AdultPlotLockOwner, "Toggle the 18+ lock on your plot.", "nsfwlock")]
        public void HandleHouseNSFWLock(ICommandContext context,
            [Parameter("On or off")]
            string setting)
        {
            bool setLock;
            if(setting.Equals("on", StringComparison.InvariantCultureIgnoreCase))
            {
                setLock = true;
            }
            else if(setting.Equals("off", StringComparison.InvariantCultureIgnoreCase))
            {
                setLock = false;
            }
            else
            {
                context.SendError("Setting was not 'on' or 'off'.");
                return;
            }
            Residence res = GlobalResidenceManager.Instance.GetResidenceByOwner(context.InvokingPlayer.Name);
            if (res != null)
            {
                bool result = res.Set18PlusLock(setLock);
                if(!result)
                {
                    context.SendError("Could not enable lock. Is there anyone on the plot that is not 18+?");
                }
            }
        }

        [Command(Permission.AdultPlotLockOwner, "Set the time limit on the 18+ lock on your plot.", "nsfwtimelock")]
        public void HandleHouseNSFWTimeLock(ICommandContext context,
           [Parameter("How long?")]
            string time,
           [Parameter("Time unit (minute, hour, day, week, month)")]
            string timeUnit)
        {
            DateTime lockTime = DateTime.Now;
            string timeAmount = "";
            if (!string.IsNullOrWhiteSpace(timeUnit))
            {
                if(uint.TryParse(time, out uint timeNum))
                {
                    switch(timeUnit.ToLowerInvariant())
                    {
                        case "minute":
                        case "min":
                        case "minutes":
                        case "mins":
                            lockTime = lockTime.AddMinutes(timeNum);
                            timeAmount = $"{timeNum} {(timeNum != 1 ? "minutes" : "minute")}";
                            break;
                        case "hour":
                        case "hours":
                            lockTime = lockTime.AddHours(timeNum);
                            timeAmount = $"{timeNum} {(timeNum != 1 ? "hours" : "hour")}";
                            break;
                        case "day":
                        case "days":
                            lockTime = lockTime.AddDays(timeNum);
                            timeAmount = $"{timeNum} {(timeNum != 1 ? "days" : "day")}";
                            break;
                        case "week":
                        case "weeks":
                            lockTime = lockTime.AddDays(timeNum * 7);
                            timeAmount = $"{timeNum} {(timeNum != 1 ? "weeks" : "week")}";
                            break;
                        case "month":
                        case "months":
                            lockTime = lockTime.AddMonths((int) timeNum);
                            timeAmount = $"{timeNum} {(timeNum != 1 ? "months" : "month")}";
                            break;
                        default:
                            context.SendError("Time unit not recognized, should be minute/hour/day/week/month");
                            return;
                    }
                }
                else
                {
                    context.SendError("Could not parse the first parameter.");
                    return;
                }
            }
            else
            {
                context.SendError("Time unit not defined.");
            }


            Residence res = GlobalResidenceManager.Instance.GetResidenceByOwner(context.InvokingPlayer.Name);
            if (res != null)
            {
                bool result = res.Set18PlusLock(true, lockTime, timeAmount);
                if (!result)
                {
                    context.SendError("Could not enable lock. Is there anyone on the plot that is not 18+?");
                }
            }
        }

        [Command(Permission.HouseRemodel, "Change ground/sky.", "remodel")]
        public void HandleRemodelCommand(ICommandContext context,
            [Parameter("Ground, sky, music, or house plug?")]
            string option,
            [Parameter("ID")]
            ushort id)
        {
            try
            {
                Player target = context.InvokingPlayer;
                //remodel
                ClientHousingRemodel clientRemod = new ClientHousingRemodel();
                ResidenceMapInstance residenceMap = target.Map as ResidenceMapInstance;
                if (residenceMap == null)
                {
                    context.SendError("You need to be on a housing map to use this command!");
                }

                Residence residence = GlobalResidenceManager.Instance.GetResidenceByOwner(context.InvokingPlayer.Name);

                if (option.ToLower() == "ground")
                {
                    residence.Ground = id;
                    log.Trace($"{target.Name} requesting to remodel: ground ID {id}.");
                }
                else if (option.ToLower() == "sky")
                {
                    residence.Sky = id;
                    log.Trace($"{target.Name} requesting to remodel: sky ID {id}.");
                }
                else if (option.ToLower() == "music")
                {
                    residence.Music = id;
                    log.Trace($"{target.Name} requesting to remodel: music ID {id}.");
                }
                else if (option.ToLower() == "house")
                {
                    var plugItem = GameTableManager.Instance.HousingPlugItem.GetEntry(id);
                    if(plugItem != null)
                    {
                        ClientHousingPlugUpdate pu = new ClientHousingPlugUpdate
                        {
                            Operation = PlugUpdateOperation.Place,
                            PlotInfo = residence.GetPlot(0).PlotInfoEntry.Id,
                            PlugFacing = (uint)residence.GetPlot(0).PlugFacing,
                            PlugItem = id,
                            ResidenceId = residence.Id,
                            RealmId = WorldServer.RealmId
                        };
                        residence.getMap().SetPlug(residence, target, pu);
                        /*if (residence.SetHouse(plugItem))
                        {
                            residence.getMap().HandleHouseChange(target, residence.GetPlot(0));
                        }
                        else
                        {
                            context.SendError("Unknown error.");
                            return;
                        }*/
                    }
                    else
                    {
                        context.SendError("Invalid housingPlugItem ID.");
                        return;
                    }
                    log.Trace($"{target.Name} requesting to remodel: house plug ID {id}.");
                    return; // not a normal remodel.
                }
                else
                {
                    context.SendError("You can only change the ground, sky, music, or house plug with this command.");
                }
                residenceMap.Remodel(new Network.Message.Model.Shared.TargetResidence
                {
                    ResidenceId = residence.Id,
                    RealmId = WorldServer.RealmId
                }, target, clientRemod);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in HouseCommandCategory.HandleRemodelCommand!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }
    }
}
