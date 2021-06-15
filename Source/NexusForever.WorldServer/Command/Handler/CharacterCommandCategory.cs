using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.RBAC.Static;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Character, "A collection of commands to manage a character.", "character")]
    [CommandTarget(typeof(Player))]
    public class CharacterCommandCategory : CommandCategory
    {
        [Command(Permission.CharacterXP, "Add XP to character.", "xp")]
        public void HandleCharacterXP(ICommandContext context,
            [Parameter("Amount of XP to grant character.")]
            uint amount)
        {
            Player target = context.GetTargetOrInvoker<Player>();
            if (target.Level >= 50)
            {
                context.SendMessage("You must be less than max level.");
                return;
            }

            target.GrantXp(amount);
        }

        [Command(Permission.CharacterLevel, "Add level to character", "level")]
        public void HandleCharacterLevel(ICommandContext context,
            [Parameter("Level to set character.")]
            byte level)
        {
            Player target = context.GetTargetOrInvoker<Player>();
            if (level <= target.Level || level > 50)
            {
                context.SendMessage("Level must be greater than your current level and less than max level.");
                return;
            }

            target.XpManager.SetLevel(level);
        }

        [Command(Permission.CharacterSave, "Save any pending changes to the character to the database.", "save")]
        public void HandleCharacterSave(ICommandContext context)
        {
            context.GetTargetOrInvoker<Player>().Save();
        }

        [Command(Permission.CharacterProps, "[propertyname] [amount] - change character properties", "props")]
        public void HandleProps(ICommandContext context,
           [Parameter("Property name.")]
            string prop,
           [Parameter("Property value.")]
            float val)
        {
            Player target = context.GetTargetOrInvoker<Player>();
            switch(prop.ToLower())
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

        [Command(Permission.CharacterProps, "Reset character properties.", "resetprops")]
        public void HandleCharacterLevel(ICommandContext context)
        {
            Player target = context.GetTargetOrInvoker<Player>();
            target.SetProperty(Property.MoveSpeedMultiplier, 1f, 1f);
            target.SetProperty(Property.MountSpeedMultiplier, 2f, 2f);
            target.SetProperty(Property.GravityMultiplier, 1f, 1f);
            target.SetProperty(Property.JumpHeight, 5f, 5f);
        }
    }
}
