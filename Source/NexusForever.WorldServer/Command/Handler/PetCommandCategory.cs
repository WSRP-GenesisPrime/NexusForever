using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Helper;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.RBAC.Static;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Pet, "A collection of commands to managed pets for a character.", "pet")]
    [CommandTarget(typeof(Player))]
    public class PetCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.PetUnlockFlair, "Unlock a pet flair for character.", "unlockflair")]
        public void HandlePetUnlockFlair(ICommandContext context,
            [Parameter("Pet flair entry id to unlock.")]
            ushort petFlairEntryId)
        {
            try
            {
                context.InvokingPlayer.PetCustomisationManager.UnlockFlair(petFlairEntryId);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in HouseCommandCategory.HandlePetUnlockFlair!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.Pet, "Dismiss pet.", "dismiss")]
        public void HandlePetDismiss(ICommandContext context)
        {
            context.InvokingPlayer?.DestroyPet();
        }

        [Command(Permission.Pet, "Make pet stay.", "stay")]
        public void HandlePetStay(ICommandContext context)
        {
            Player target = context.InvokingPlayer;
            target.SetPetFollowing(false);
            target.SetPetFacingPlayer(false);
        }

        [Command(Permission.Pet, "Make pet follow by your side.", "side")]
        public void HandlePetSide(ICommandContext context)
        {
            Player target = context.InvokingPlayer;
            target.SetPetFollowingOnSide(true);
            target.SetPetFacingPlayer(false);
        }

        [Command(Permission.Pet, "Make pet follow behind you.", "behind")]
        public void HandlePetBehind(ICommandContext context)
        {
            Player target = context.InvokingPlayer;
            target.SetPetFollowingOnSide(false);
            target.SetPetFacingPlayer(true);
        }

        [Command(Permission.Pet, "Make pet follow you.", "follow")]
        public void HandlePetFollow(ICommandContext context,
            [Parameter("Distance (short/medium/long).", Static.ParameterFlags.Optional)]
            string distanceParameter)
        {
            try
            {
                Player target = context.InvokingPlayer;

                float followDistance = 4f;
                float recalcDistance = 5f;

                distanceParameter = distanceParameter?.ToLower();

                switch (distanceParameter)
                {
                    case "short":
                        followDistance = 1f;
                        recalcDistance = 1f;
                        break;
                    default:
                        followDistance = 4f;
                        recalcDistance = 5f;
                        break;
                    case "long":
                        followDistance = 7f;
                        recalcDistance = 9f;
                        break;
                }

                target.SetPetFollowing(true);
                target.SetPetFacingPlayer(true);
                target.SetPetFollowDistance(followDistance);
                target.SetPetFollowRecalculateDistance(recalcDistance);
                context.SendMessage($"Vanity pet set to follow: {target.Name}, Follow distance: {distanceParameter}");

                return;
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in HouseCommandCategory.HandlePetFollow!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.Pet, "Summon pet.", "summon")]
        public void HandlePetSummon(ICommandContext context,
            [Parameter("Creature type.")]
            string creatureType,
            [Parameter("Creature variant.", Static.ParameterFlags.Optional)]
            string creatureVariant)
        {
            Player target = context.InvokingPlayer;
            if (target.VanityPetGuid != null)
            {
                context.SendError("You already have a pet - please dismiss it before summoning another.");
                return;
            }

            context.SendMessage($"Getting {creatureType.ToUpper()} variant: {creatureVariant}");

            try
            {
                uint? id = CreatureHelper.GetCreatureIdFromType(creatureType, creatureVariant);
                if(id == null)
                {
                    context.SendError("Creature not found!");
                    return;
                }

                bool storyTellerOnly = CreatureHelper.IsStoryTellerOnly(creatureType);
                if (storyTellerOnly && !target.Session.AccountRbacManager.HasPermission(Permission.MorphStoryteller))
                {
                    context.SendError($"Your account lacks permission to use this Storyteller Only summon: {creatureType}");
                    return;
                }

                SummonCreatureToPlayer(context, (uint) id);
            }
            catch (TypeInitializationException tie)
            {
                log.Error($"Exception caught in HouseCommandCategory.HandlePetSummon!\nInvoked by {context.InvokingPlayer.Name}; {tie.Message} : {tie.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        public void SummonCreatureToPlayer(ICommandContext context, uint creatureId)
        {
            Player target = context.InvokingPlayer;
            Creature2Entry creature2 = GameTableManager.Instance.Creature2.GetEntry(creatureId);

            if (creature2 == null || creatureId == 0)
            {
                context.SendError($"Invalid summon variant.");
                return;
            }

            var tempEntity = new VanityPet(target, creatureId);
            Creature2OutfitGroupEntryEntry outfitGroupEntry = System.Linq.Enumerable.FirstOrDefault(GameTableManager.Instance.Creature2OutfitGroupEntry.Entries, d => d.Creature2OutfitGroupId == creature2.Creature2OutfitGroupId);

            if (outfitGroupEntry != null)
            {
                tempEntity.SetDisplayInfo(tempEntity.DisplayInfo, outfitGroupEntry.Creature2OutfitInfoId);
            }
            log.Info($"Summoning entity {creature2.Id}: '{creature2.Description}' to {target.Name} @ ({target.Position}, {target.Zone.Id})");
            target.Map.EnqueueAdd(tempEntity, new MapPosition
            {
                Position = target.Position
            });
        }
    }
}
