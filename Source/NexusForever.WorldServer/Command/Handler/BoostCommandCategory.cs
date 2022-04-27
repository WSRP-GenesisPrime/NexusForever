using System.Text;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Network.Message.Model;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Boost, "Character Boosts and Unlocks.", "boost")]
    [CommandTarget(typeof(Player))]
    public class BoostCommandCategory : CommandCategory
    {
        [Command(Permission.Boost, "Boosts your character to level 50, restart client for it to take effect.", "level")]
        public void HandleBoostLevel(ICommandContext context)
        {
            Player target = context.InvokingPlayer;
            if (target.Level < 50)
            {
                target.XpManager.SetLevel(50);
            }
        }

        [Command(Permission.Boost, "Grants some character currencies.", "money")]
        public void HandleBoostMoney(ICommandContext context)
        {
            Player target = context.InvokingPlayer;
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Credits, 500000000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Renown, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.ElderGems, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.CraftingVoucher, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Prestige, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Glory, 500000);
        }

        [Command(Permission.Boost, "Unlock lore.", "lore")]
        public void HandleBoostLore(ICommandContext context)
        {
            Player target = context.InvokingPlayer;

            target.DatacubeManager.UnlockAllLore();
        }

        [Command(Permission.Boost, "Level boost, currencies, unlock all dyes and unlock all lore.", "all")]
        public void HandleBoostAll(ICommandContext context)
        {
            Player target = context.InvokingPlayer;

            if (target.Level < 50)
            {
                target.XpManager.SetLevel(50);
            }

            target.Session.GenericUnlockManager.UnlockAll(GenericUnlockType.Dye);

            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Credits, 500000000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Renown, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.ElderGems, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.CraftingVoucher, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Prestige, 500000);
            target.CurrencyManager.CurrencyAddAmount(CurrencyType.Glory, 500000);

            target.DatacubeManager.UnlockAllLore();
        }
    }
}
