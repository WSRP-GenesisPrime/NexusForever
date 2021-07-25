using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Game.Spell;
using NLog;
using System;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Character, "A collection of commands to manage a character.", "character")]
    [CommandTarget(typeof(Player))]
    public class CharacterCommandCategory : CommandCategory
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        [Command(Permission.CharacterXP, "Add XP to character.", "xp")]
        public void HandleCharacterXP(ICommandContext context,
            [Parameter("Amount of XP to grant character.")]
            uint amount)
        {
            try
            {
                Player target = context.InvokingPlayer;
                if (target.Level >= 50)
                {
                    context.SendMessage("You must be less than max level.");
                    return;
                }

                target.GrantXp(amount);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in CharacterCommandCategory.HandleCharacterXP!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.CharacterLevel, "Add level to character", "level")]
        public void HandleCharacterLevel(ICommandContext context,
            [Parameter("Level to set character.")]
            byte level)
        {
            try
            {
                Player target = context.InvokingPlayer;
                if (level <= target.Level || level > 50)
                {
                    context.SendMessage("Level must be greater than your current level and less than max level.");
                    return;
                }

                target.XpManager.SetLevel(level);
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in CharacterCommandCategory.HandleCharacterLevel!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }

        }

        [Command(Permission.CharacterSave, "Save any pending changes to the character to the database.", "save")]
        public void HandleCharacterSave(ICommandContext context)
        {
            context.InvokingPlayer.Save();
        }

        [Command(Permission.CharacterProps, "[propertyname] [amount] - change character properties", "props")]
        public void HandleProps(ICommandContext context,
           [Parameter("Property name.")]
            string prop,
           [Parameter("Property value.")]
            float val)
        {
            try
            {
                Player target = context.InvokingPlayer;
                switch (prop.ToLower())
                {
                    case "speed":
                        if (val < 1f)
                        {
                            context.SendError("Speed multiplier can not be below 1.");
                            return;
                        }
                        else if (val > 8f)
                        {
                            context.SendError("Speed multiplier can not be above 8.");
                            return;
                        }
                        else
                        {
                            target.SetProperty(Property.MoveSpeedMultiplier, val, val);
                            context.SendMessage("Gotta go fast!");
                        }
                        break;
                    case "mountspeed":
                        if (val < 1f)
                        {
                            context.SendError("Mount speed multiplier can not be below 1.");
                            return;
                        }
                        else if (val > 5f)
                        {
                            context.SendError("Mount speed multiplier can not be above 5.");
                            return;
                        }
                        else
                        {
                            target.SetProperty(Property.MountSpeedMultiplier, val, val);
                            context.SendMessage("Nyooom!");
                        }
                        break;
                    case "gravity":
                        if (val < 0.1f)
                        {
                            context.SendError("Gravity multiplier can not be below 0.1.");
                            return;
                        }
                        else if (val > 5f)
                        {
                            context.SendError("Gravity multiplier can not be above 5.");
                            return;
                        }
                        else
                        {
                            target.SetProperty(Property.GravityMultiplier, val, val);
                            context.SendMessage("Like a feather!");
                        }
                        break;
                    case "jump":
                        if (val < 1f)
                        {
                            context.SendError("Jump height can not be below 1.");
                            return;
                        }
                        else if (val > 20f)
                        {
                            context.SendError("Jump height can not be above 20.");
                            return;
                        }
                        else
                        {
                            target.SetProperty(Property.JumpHeight, val, val);
                            context.SendMessage("A giant leap for whatever-you-are!");
                        }
                        break;
                    default:
                        context.SendMessage("I'm afraid I can't let you do that!\nValid props are: speed, mountspeed, gravity, jump.");
                        break;
                }
            }
            catch (Exception e)
            {
                log.Error($"Exception caught in CharacterCommandCategory.HandleProps!\nInvoked by {context.InvokingPlayer.Name}; {e.Message} :\n{e.StackTrace}");
                context.SendError("Oops! An error occurred. Please check your command input and try again.");
            }
        }

        [Command(Permission.CharacterProps, "Reset character properties.", "resetprops")]
        public void HandleCharacterLevel(ICommandContext context)
        {
            Player target = context.InvokingPlayer;
            target.SetProperty(Property.MoveSpeedMultiplier, 1f, 1f);
            target.SetProperty(Property.MountSpeedMultiplier, 2f, 2f);
            target.SetProperty(Property.GravityMultiplier, 1f, 1f);
            target.SetProperty(Property.JumpHeight, 5f, 5f);
        }

        [Command(Permission.None, "grow your character", "grow")]
        public void HandleGrow(ICommandContext context,
           [Parameter("Amount of stacks.", Static.ParameterFlags.Optional)]
            uint? stacks)
        {
            if (stacks == null)
            {
                stacks = 1;
            }
            if (stacks > 20)
            {
                stacks = 20;
            }
            for (int i = 0; i < stacks; ++i)
            {
                context.GetTargetOrInvoker<UnitEntity>().CastSpell(63491, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
            }
        }

        [Command(Permission.None, "shrink your character", "shrink")]
        public void HandleShrink(ICommandContext context,
           [Parameter("Amount of stacks.", Static.ParameterFlags.Optional)]
            uint? stacks)
        {
            if (stacks == null)
            {
                stacks = 1;
            }
            if (stacks > 20)
            {
                stacks = 20;
            }
            for (int i = 0; i < stacks; ++i)
            {
                context.GetTargetOrInvoker<UnitEntity>().CastSpell(63490, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
            }
        }

        [Command(Permission.None, "scale your character", "scale")]
        public void HandleScale(ICommandContext context,
           [Parameter("Scale.")]
            float targetScale,
           [Parameter("Maximum number of stacks.", Static.ParameterFlags.Optional)]
            uint? maxStacks)
        {
            uint mStacks = maxStacks ?? 20;
            if(mStacks > 100)
            {
                mStacks = 100;
            }

            uint smallStacks = 0;
            uint largeStacks = 0;
            uint bestSmallStacks = 0;
            uint bestLargeStacks = 0;
            float bestError = float.MaxValue;
            float bestScale = 1f;

            float scale = 1f;

            while(smallStacks + largeStacks < mStacks)
            {
                if(scale < targetScale)
                {
                    largeStacks += 1;
                }
                else
                {
                    smallStacks += 1;
                }
                scale = MathF.Pow(1.5f, largeStacks) * MathF.Pow(0.8f, smallStacks);
                float error = scale / targetScale;
                if(error < 1f)
                {
                    error = 1f / error;
                }
                error -= 1f;
                if(error < bestError)
                {
                    bestError = error;
                    bestSmallStacks = smallStacks;
                    bestLargeStacks = largeStacks;
                    bestScale = scale;
                }
            }

            context.SendMessage($"Scaling you by {bestScale}x.");

            for (int i = 0; i < bestSmallStacks; ++i)
            {
                context.GetTargetOrInvoker<UnitEntity>().CastSpell(63490, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
            }
            for (int i = 0; i < bestLargeStacks; ++i)
            {
                context.GetTargetOrInvoker<UnitEntity>().CastSpell(63491, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
            }
        }
    }
}
