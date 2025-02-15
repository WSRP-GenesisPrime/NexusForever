﻿using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Helper;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Network.Message.Handler;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Emote, "Play emotes.", "emote")]
    [CommandTarget(typeof(Player))]
    public class EmoteCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.Emote, "Act out a specific emote.", "play")]
        public void HandleEmote(ICommandContext context,
            [Parameter("Emote name.")]
            string emote)
        {
            try
            {
                Player player = context.InvokingPlayer;

                emote = emote.ToLower();

                if (EmoteHelper.IsEmoteAllowedByRace(emote, (uint)player.Race))
                {
                    if (EmoteHelper.IsEmoteAllowedBySex(emote, (uint)player.Sex))
                    {
                        //emote is compatible
                    }
                    else
                    {
                        context.SendError($"{emote} is not a compatible emote with your character sex!");
                        return;
                    }
                } 
                else
                {
                    context.SendError($"{emote} is not a compatible emote with your character race!");
                    return;
                }

                uint? id = EmoteHelper.GetEmoteId(emote);
                if (id == null)
                {
                    context.SendError("An Emote ID for the given emote name could not be found!");
                    return;
                }

                ClientEmote clientEmote = new ClientEmote
                {
                    EmoteId = (uint)id,
                    Targeted = false,
                    Silent = false
                };

                SocialHandler.SetEmote(player.Session, clientEmote);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in EmoteCommandCategory.HandleEmote!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.GMFlag, "Act out a specific emote by ID.", "id")]
        public void HandleEmoteId(ICommandContext context,
            [Parameter("Emote id.")]
            uint id)
        {
            try
            {
                Player player = context.InvokingPlayer;

                if (id <= 0)
                {
                    context.SendError("Invalid emote ID.");
                    return;
                }

                ClientEmote clientEmote = new ClientEmote
                {
                    EmoteId = (uint)id,
                    Targeted = false,
                    Silent = false
                };

                SocialHandler.SetEmote(player.Session, clientEmote);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in EmoteCommandCategory.HandleEmoteId!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.Emote, "List available emotes.", "list")]
        public void HandleEmoteList(ICommandContext context)
        {
            Player player = context.InvokingPlayer;
            string message = "Emotes for your race:";
            foreach(string emote in EmoteHelper.GetEmoteList((uint)player.Race, (uint)player.Sex)) {
                message += $"\n{emote}";
            }

            context.SendMessage(message);
        }

        [Command(Permission.Emote, "Stop emote which is currently playing.", "stop")]
        public void HandleEmoteStop(ICommandContext context)
        {
            context.InvokingPlayer.EnqueueToVisible(new ServerEmote
            {
                Guid = context.InvokingPlayer.Guid,
                StandState = StandState.Stand,
            }, true);
        }
    }
}
