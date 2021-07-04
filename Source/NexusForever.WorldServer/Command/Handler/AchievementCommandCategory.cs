using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Achievement;
using NexusForever.WorldServer.Game.Achievement.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Achievement, "A collection of commands to manage player achievements.", "achievement")]
    [CommandTarget(typeof(Player))]
    public class AchievementCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.AchievementUpdate, "Update achievement criteria for player.", "update")]
        public void HandleAchievementUpdate(ICommandContext context,
            [Parameter("Achievement criteria type to update.", ParameterFlags.None, typeof(EnumParameterConverter<AchievementType>))]
            AchievementType type,
            [Parameter("Object id to match against.")]
            uint objectId,
            [Parameter("Alternative object id to match against.")]
            uint objectIdAlt,
            [Parameter("Update count for matched criteria.")]
            uint count)
        {
            try
            {
                Player player = context.InvokingPlayer;
                log.Info($"{player.Name} requesting achievement update type {type}.");
                player.AchievementManager.CheckAchievements(player, type, objectId, objectIdAlt, count);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in AchievementCommandCategory.HandleAchievementUpdate!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.AchievementGrant, "Grant achievement to player.", "grant")]
        public void HandleAchievementGrant(ICommandContext context,
            [Parameter("Achievement id to grant.")]
            ushort achievementId)
        {
            try
            {
                AchievementInfo info = GlobalAchievementManager.Instance.GetAchievement(achievementId);
                if (info == null)
                {
                    context.SendMessage($"Invalid achievement id {achievementId}!");
                    return;
                }
                log.Info($"{context.InvokingPlayer.Name} requesting achievement grant ID {achievementId}.");
                context.InvokingPlayer.AchievementManager.GrantAchievement(achievementId);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in AchievementCommandCategory.HandleAchievementGrant!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }
    }
}
