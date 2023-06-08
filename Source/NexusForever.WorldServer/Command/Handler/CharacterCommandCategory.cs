using NexusForever.Shared.Database;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Game.RealmTask.Static;
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
            Player target = context.InvokingPlayer;
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
            Player target = context.InvokingPlayer;
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
            context.InvokingPlayer.Save();
        }

        [Command(Permission.Character, "[newName] - Rename your character. This change will take effect after the next server restart.", "rename")]
        public void HandleCharacterRename(ICommandContext context,
           [Parameter("The new name you want for your character")]
            string newName)
        {
            
            Player target = context.InvokingPlayer;
            string accountName = target.Session.Account.Email;
            uint characterId = (uint) target.CharacterId;
            log.Info($"HandleCharacterRename(newName={newName},characterId={characterId})");
            DatabaseManager.Instance.WorldDatabase.CreateRealmTask((uint)RealmTaskType.CharacterRename, newName, characterId, 0, 0, 0, null, accountName);
            context.SendMessage("Your character rename has been staged. The change will take effect after the next server restart.");
        }

            [Command(Permission.CharacterProps, "[propertyname] [amount] - change character properties", "props")]
        public void HandleProps(ICommandContext context,
           [Parameter("Property name.")]
            string prop,
           [Parameter("Property value.")]
            float val)
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
                        target.SetBaseProperty(Property.MoveSpeedMultiplier, val);
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
                        target.SetBaseProperty(Property.MountSpeedMultiplier, val);
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
                        target.SetBaseProperty(Property.GravityMultiplier, val);
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
                        target.SetBaseProperty(Property.JumpHeight, val);
                        context.SendMessage("A giant leap for whatever-you-are!");
                    }
                    break;
                case "slowfall":
                    if (val < 0f)
                    {
                        context.SendError("Slowfall multiplier can not be below 0.");
                        return;
                    }
                    else
                    {
                        target.SetBaseProperty(Property.SlowFallMultiplier, val);
                        context.SendMessage("I believe I can flyyyyy...");
                    }
                    break;
                case "friction":
                    if (val < 0f)
                    {
                        context.SendError("Friction can not be below 0.");
                        return;
                    }
                    else
                    {
                        target.SetBaseProperty(Property.FrictionMax, val);
                        context.SendMessage("Grippy!");
                    }
                    break;
                default:
                    context.SendMessage("I'm afraid I can't let you do that!\nValid props are: speed, mountspeed, gravity, jump.");
                    break;
            }
        }

        [Command(Permission.CharacterProps, "Reset character properties.", "resetprops")]
        public void HandleCharacterResetProps(ICommandContext context)
        {
            Player target = context.InvokingPlayer;
            target.SetBaseProperty(Property.MoveSpeedMultiplier, 1f);
            target.SetBaseProperty(Property.MountSpeedMultiplier, 2f);
            target.SetBaseProperty(Property.GravityMultiplier, 1f);
            target.SetBaseProperty(Property.JumpHeight, 5f);
            target.SetBaseProperty(Property.SlowFallMultiplier, 1f);
            target.SetBaseProperty(Property.FrictionMax, 1f);
        }

        [Command(Permission.GMFlag, "grow your character", "grow")]
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

        [Command(Permission.GMFlag, "shrink your character", "shrink")]
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
            Player target = context.InvokingPlayer;
            target.WipeEffectsByID(63490);
            target.WipeEffectsByID(63491);
            uint mStacks = maxStacks ?? 20;
            if(mStacks > 100)
            {
                mStacks = 100;
            }

            uint smallStacks = 0;
            uint largeStacks = 0;
            uint bestSmallStacks = 0;
            uint bestLargeStacks = 0;
            float bestError = targetScale;
            if (bestError < 1f)
            {
                bestError = 1 / bestError;
            }
            bestError -= 1f;
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

            target.TargetGuid = target.EntityId;

            for (int i = 0; i < bestSmallStacks; ++i)
            {
                target.CastSpell(63490, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false,
                    PositionalUnitId = target.Guid
                });
            }
            for (int i = 0; i < bestLargeStacks; ++i)
            {
                target.CastSpell(63491, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false,
                    PositionalUnitId = target.Guid
                });
            }
        }

        [Command(Permission.MorphStoryteller, "scale another character", "scaleOther")]
        public void HandleScaleOther(ICommandContext context,
           [Parameter("Scale.")]
            float targetScale,
           [Parameter("Maximum number of stacks.", Static.ParameterFlags.Optional)]
            uint? maxStacks)
        {
            UnitEntity target = (UnitEntity) context.Target;
            target.WipeEffectsByID(63490);
            target.WipeEffectsByID(63491);
            uint mStacks = maxStacks ?? 20;
            if (mStacks > 100)
            {
                mStacks = 100;
            }

            uint smallStacks = 0;
            uint largeStacks = 0;
            uint bestSmallStacks = 0;
            uint bestLargeStacks = 0;
            float bestError = targetScale;
            if (bestError < 1f)
            {
                bestError = 1 / bestError;
            }
            bestError -= 1f;
            float bestScale = 1f;

            float scale = 1f;

            while (smallStacks + largeStacks < mStacks)
            {
                if (scale < targetScale)
                {
                    largeStacks += 1;
                }
                else
                {
                    smallStacks += 1;
                }
                scale = MathF.Pow(1.5f, largeStacks) * MathF.Pow(0.8f, smallStacks);
                float error = scale / targetScale;
                if (error < 1f)
                {
                    error = 1f / error;
                }
                error -= 1f;
                if (error < bestError)
                {
                    bestError = error;
                    bestSmallStacks = smallStacks;
                    bestLargeStacks = largeStacks;
                    bestScale = scale;
                }
            }

            for (int i = 0; i < bestSmallStacks; ++i)
            {
                target.CastSpell(63490, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
            }
            for (int i = 0; i < bestLargeStacks; ++i)
            {
                target.CastSpell(63491, 1, new SpellParameters
                {
                    UserInitiatedSpellCast = false
                });
            }
        }

        [Command(Permission.Morph, "Mount a creature by name.", "mount")]
        public void HandleMount(ICommandContext context,
           [Parameter("Creature ID.")]
            uint creature2ID,
           [Parameter("Vehicle ID.", Static.ParameterFlags.Optional)]
            uint? vehicleID)
        {
            Player player = context.InvokingPlayer;

            if (player.VehicleGuid != 0)
            {
                player.Dismount();
            }

            if (!player.CanMount())
                return;

            var mount = new Mount(player, 82107, creature2ID, vehicleID ?? 1, 0, 0);
            mount.EnqueuePassengerAdd(player, VehicleSeatType.Pilot, 0);

            // usually for hover boards
            /*if (info.Entry.DataBits04 > 0u)
            {
                mount.SetAppearance(new ItemVisual
                {
                    Slot      = ItemSlot.Mount,
                    DisplayId = (ushort)info.Entry.DataBits04
                });
            }*/

            var position = new MapPosition
            {
                Position = player.Position
            };

            if (player.Map.CanEnter(mount, position))
                player.Map.EnqueueAdd(mount, position);

            // FIXME: also cast 52539,Riding License - Riding Skill 1 - SWC - Tier 1,34464
            // FIXME: also cast 80530,Mount Sprint  - Tier 2,36122
        }
    }
}
