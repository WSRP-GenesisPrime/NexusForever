using System.Collections.Generic;
using System.Text;
using NLog;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Command.Helper;
using NexusForever.WorldServer.Game.RBAC;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Morph, "A collection of commands to change into creatures.", "morph")]
    [CommandTarget(typeof(Player))]
    public class MorphCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.Morph, "Change into a creature.", "into")]
        public void HandleMorph(ICommandContext context,
            [Parameter("Creature type.")]
            string type,
            [Parameter("Creature subtype.")]
            string subtype)
        {
            Player player = context.GetTargetOrInvoker<Player>();

            type = type.ToLower();
            subtype = subtype.ToLower();

            bool storyTellerOnly = CreatureHelper.isStoryTellerOnly(type);
            if(storyTellerOnly && !player.Session.AccountRbacManager.HasPermission(Permission.MorphStoryteller))
            {
                context.SendError("This creature category is only for storytellers and game masters!");
                return;
            }

            uint? creatureID = CreatureHelper.GetCreatureIdFromType(type, subtype);
            if(creatureID == null)
            {
                context.SendError("A Creature ID for the given type and variant could not be found!");
                return;
            }
            
            Creature2Entry creature2 = GameTableManager.Instance.Creature2.GetEntry((ulong) creatureID);
            if (creature2 == null || creatureID == 0)
            {
                log.Info($"Invalid morph variant.");
                return;
            }

            Creature2DisplayGroupEntryEntry displayGroupEntry = System.Linq.Enumerable.FirstOrDefault(GameTableManager.Instance.Creature2DisplayGroupEntry.Entries, (d => d.Creature2DisplayGroupId == creature2.Creature2DisplayGroupId));
            if (displayGroupEntry == null)
                return;

            // change the player's display information to the creature's display information
            Creature2OutfitGroupEntryEntry outfitGroupEntry = System.Linq.Enumerable.FirstOrDefault(GameTableManager.Instance.Creature2OutfitGroupEntry.Entries, (d => d.Creature2OutfitGroupId == creature2.Creature2OutfitGroupId));


            if (outfitGroupEntry != null) // check if the creature has an outfit
            {
                player.SetDisplayInfo(displayGroupEntry.Creature2DisplayInfoId, outfitGroupEntry.Creature2OutfitInfoId); // if there is outfit information, use outfit info parameter
            }
            else
            {
                player.SetDisplayInfo(displayGroupEntry.Creature2DisplayInfoId);
            }
        }

        [Command(Permission.Morph, "Change back to your usual amazing self.", "demorph")]
        public void HandleDemorph(ICommandContext context)
        {
            Player p = context.GetTargetOrInvoker<Player>();
            p.ResetAppearance();
        }

        [Command(Permission.Morph, "List morphs.", "list")]
        public void HandleMorphList(ICommandContext context,
            [Parameter("Creature type.", ParameterFlags.Optional)]
            string type)
        {
            Player player = context.GetTargetOrInvoker<Player>();

            bool storyteller = player.Session.AccountRbacManager.HasPermission(Permission.MorphStoryteller);
            if (type != null && (!CreatureHelper.isStoryTellerOnly(type) || storyteller))
            {
                List<string> variants = CreatureHelper.getCreatureVariantsForType(type);
                if(variants != null && variants.Count > 0)
                {
                    string message = string.Format("Morph list: {0}", type);
                    foreach(string entry in variants)
                    {
                        message = message + "\n" + entry;
                    }
                    context.SendMessage(message);
                }
            }
            else
            {
                List<string> types = CreatureHelper.getCreatureTypeList(storyteller);
                if(types != null && types.Count > 0)
                {
                    string message = string.Format("Available morph types:");
                    foreach(string entry in types)
                    {
                        message = message + "\n" + entry;
                    }
                    context.SendMessage(message);
                }
            }
        }
    }
}
