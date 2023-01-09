using System.Linq;
using System.Text;
using NexusForever.Shared.GameTable;
using NexusForever.WorldServer.Command.Context;
using NexusForever.WorldServer.Command.Convert;
using NexusForever.WorldServer.Command.Static;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game.RBAC.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Social.Static;

namespace NexusForever.WorldServer.Command.Handler
{
    [Command(Permission.Quest, "A collection of commands to manage quests for a character.", "quest")]
    [CommandTarget(typeof(Player))]
    public class QuestCommandCategory : CommandCategory
    {
        [Command(Permission.QuestList, "List all active quests.", "list")]
        public void HandleQuestList(ICommandContext context)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Active quests for {context.GetTargetOrInvoker<Player>().Name}");
            builder.AppendLine("=============================");
            context.SendMessage(builder.ToString());
            foreach (Quest quest in context.GetTargetOrInvoker<Player>().QuestManager.GetActiveQuests())
            {
                var chatBuilder = new ChatMessageBuilder
                {
                    Type = ChatChannelType.System,
                    Text = $"({quest.Id}) "
                };
                chatBuilder.AppendQuest(quest.Id);
                context.GetTargetOrInvoker<Player>().Session.EnqueueMessageEncrypted(chatBuilder.Build());
            }
        }

        [Command(Permission.QuestAdd, "Add a new quest to character.", "add")]
        public void HandleQuestAdd(ICommandContext context,
            [Parameter("Quest entry id to add to character.")]
            ushort questId)
        {
            QuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
            {
                context.SendMessage($"Quest id {questId} is invalid!");
                return;
            }

            context.InvokingPlayer.QuestManager.QuestAdd(info);
        }

        [Command(Permission.QuestAchieve, "Achieve an existing quest by completing all objectives for character.", "achieve")]
        public void HandleQuestAchieve(ICommandContext context,
            [Parameter("Quest entry id to achieve for character.")]
            ushort questId)
        {
            QuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
            {
                context.SendMessage($"Quest id {questId} is invalid!");
                return;
            }

            context.InvokingPlayer.QuestManager.QuestAchieve(questId);
        }

        [Command(Permission.QuestAchieveObjective, "Achieve a single objective for an existing quest for character.", "achieveobjective")]
        public void HandleQuestAchieveObjective(ICommandContext context,
            [Parameter("Quest entry id to achieve for character.")]
            ushort questId,
            [Parameter("Quest objective index to achieve for character.")]
            byte index)
        {
            QuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
            {
                context.SendMessage($"Quest id {questId} is invalid!");
                return;
            }

            context.InvokingPlayer.QuestManager.QuestAchieveObjective(questId, index);
        }

        [Command(Permission.QuestObjective, "Update all quest objectives with type for character.", "objective")]
        public void HandleQuestObjective(ICommandContext context,
            [Parameter("Quest objective type for objectives to update.", ParameterFlags.None, typeof(EnumParameterConverter<QuestObjectiveType>))]
            QuestObjectiveType type,
            [Parameter("Data value to match quest objectives against.")]
            uint data,
            [Parameter("Progress to increment matching quest objectives.")]
            uint? progress)
        {
            progress ??= 1u;
            context.InvokingPlayer.QuestManager.ObjectiveUpdate(type, data, progress.Value);
        }

        [Command(Permission.QuestKill, "Update all quest objectives that require a kill with the given creature id.", "kill")]
        public void HandleQuestKill(ICommandContext context,
            [Parameter("Creature id to match quest objectives against.")]
            uint creatureId,
            [Parameter("Quantity to update quest objectives by.")]
            uint? quantity)
        {
            quantity ??= 1u;

            var target = context.InvokingPlayer;
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillCreature, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillCreature2, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillCreature, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillCreature2, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillTargetGroup, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.KillTargetGroups, creatureId, quantity.Value);

            context.SendMessage($"Success! You've killed {quantity} of Creature ID: {creatureId}");
        }

        [Command(Permission.QuestActivate, "Update all quest objectives that require activating a given creature id.", "activate")]
        public void HandleQuestActivate(ICommandContext context,
            [Parameter("Creature id to match quest objectives against.")]
            uint creatureId,
            [Parameter("Quantity to update quest objectives by.")]
            uint? quantity)
        {
            quantity ??= 1u;

            var target = context.GetTargetOrInvoker<Player>();
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateEntity, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateEntity2, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateTargetGroup, creatureId, quantity.Value);
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateTargetGroupChecklist, creatureId, quantity.Value); // quantity.Value should be the QuestChecklistIdx of the entity
            target.QuestManager.ObjectiveUpdate(QuestObjectiveType.SucceedCSI, creatureId, quantity.Value);

            context.SendMessage($"Success! You've activated {quantity} of Creature ID: {creatureId}");
        }
    }
}
