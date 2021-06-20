using System.Text;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Housing;
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
            quantity ??= 1u;

            if (!(context.InvokingPlayer.Map is ResidenceMap residenceMap))
            {
                context.SendMessage("You need to be on a housing map to use this command!");
                return;
            }

            HousingDecorInfoEntry entry = GameTableManager.Instance.HousingDecorInfo.GetEntry(decorInfoId);
            if (entry == null)
            {
                context.SendMessage($"Invalid decor info id {decorInfoId}!");
                return;
            }

            residenceMap.DecorCreate(entry, quantity.Value);
        }

        [Command(Permission.HouseDecorLookup, "Returns a list of decor ids that match the supplied name.", "decorlookup")]
        public void HandleHouseDecorLookup(ICommandContext context,
            [Parameter("Name or partial name of the housing decor item to search for.")]
            string name)
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

        [Command(Permission.HouseTeleport, "Teleport to a residence, optionally specifying a character.", "teleport")]
        public void HandleHouseTeleport(ICommandContext context,
            [Parameter("", ParameterFlags.Optional)]
            string firstName,
            [Parameter("", ParameterFlags.Optional)]
            string lastName)
        {
            Player target = context.InvokingPlayer;
            if (!target.CanTeleport())
            {
                context.SendMessage("You have a pending teleport! Please wait to use this command.");
                return;
            }

            log.Trace($"{target.Name} is requesting a teleport to plot {firstName} {lastName}.");

            string name = $"{firstName} {lastName}";
            if(firstName == null && lastName == null)
            {
                name = target.Name;
            }

            Residence residence = ResidenceManager.Instance.GetResidence(name).GetAwaiter().GetResult();
            if (residence == null)
            {
                if (firstName == null && lastName == null)
                {
                    residence = ResidenceManager.Instance.CreateResidence(target);
                    log.Info($"Creating residence {residence.Id} for name {name}, firstname {firstName}, lastname {lastName}.");
                }
                else
                {
                    context.SendMessage("A residence for that character doesn't exist!");
                    return;
                }
            }

            ResidenceEntrance entrance = ResidenceManager.Instance.GetResidenceEntrance(residence);
            target.TeleportTo(entrance.Entry, entrance.Position, 0u, residence.Id);
        }

        [Command(Permission.HouseRemodel, "Change ground/sky.", "remodel")]
        public void HandleRemodelCommand(ICommandContext context,
            [Parameter("Ground or sky?")]
            string option,
            [Parameter("ID")]
            ushort id)
        {
            Player target = context.InvokingPlayer;
            //remodel
            ClientHousingRemodel clientRemod = new ClientHousingRemodel();
            ResidenceMap residenceMap = target.Map as ResidenceMap;
            if (residenceMap == null)
            {
                context.SendError("You need to be on a housing map to use this command!");
            }

            Residence residence = ResidenceManager.Instance.GetResidence(target.Name).GetAwaiter().GetResult();

            if (option.ToLower() == "ground")
            {
                residence.Ground = id;
            }
            else if (option.ToLower() == "sky")
            {
                residence.Sky = id;
            }
            else
            {
                context.SendError("You can only change the ground or sky with this command.");
            }

            residenceMap.Remodel(target, clientRemod);
        }
    }
}
