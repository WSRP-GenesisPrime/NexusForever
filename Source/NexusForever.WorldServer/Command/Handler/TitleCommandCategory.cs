using NexusForever.Shared.GameTable;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Title, "A collection of commands to manage titles for a character.", "title")]
    [CommandTarget(typeof(Player))]
    public class TitleCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.TitleAdd, "Add a title to character.", "add")]
        public void HandleTitleAdd(ICommandContext context,
            [Parameter("")]
            ushort characterTitleId)
        {
            try
            {
                if (GameTableManager.Instance.CharacterTitle.GetEntry(characterTitleId) == null)
                {
                    context.SendMessage($"Invalid character title id {characterTitleId}!");
                    return;
                }

                context.InvokingPlayer.TitleManager.AddTitle(characterTitleId);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in TitleCommandCategory.HandleTitleAdd!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.TitleRevoke, "evoke a title from character.", "revoke")]
        public void HandleTitleRemove(ICommandContext context,
            [Parameter("")]
            ushort characterTitleId)
        {
            try
            {
                if (GameTableManager.Instance.CharacterTitle.GetEntry(characterTitleId) == null)
                {
                    context.SendMessage($"Invalid character title id {characterTitleId}!");
                    return;
                }

                context.InvokingPlayer.TitleManager.RevokeTitle(characterTitleId);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in TitleCommandCategory.HandleTitleRemove!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.TitleAll, "Add all titles to character.", "all")]
        public void HandleTitleAll(ICommandContext context)
        {
            context.InvokingPlayer.TitleManager.AddAllTitles();
        }

        [Command(Permission.TitleNone, "Revoke all titles from character.", "none")]
        public void HandleTitleNone(ICommandContext context)
        {
            context.InvokingPlayer.TitleManager.RevokeAllTitles();
        }
    }
}
