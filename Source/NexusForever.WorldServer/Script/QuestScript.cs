using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Quest;
using NexusForever.WorldServer.Game.Quest.Static;

namespace NexusForever.WorldServer.Script
{
    public abstract class QuestScript : Script
    {
        public virtual void OnQuestStateChange(Player player, Quest quest, QuestState newState)
        {
        }

        public virtual void OnObjectiveUpdate(Player player, Quest quest, QuestObjective objective)
        {
        }
    }
}
