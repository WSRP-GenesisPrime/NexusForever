using NexusForever.Shared;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using NLog;

namespace NexusForever.WorldServer.Script
{
    public class ScriptManager : Singleton<ScriptManager>
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private ImmutableDictionary<uint, CreatureScript> creatureScripts;
        private ImmutableDictionary<uint, PlugScript> plugScripts;
        private ImmutableDictionary<uint, QuestScript> questScripts;

        public ScriptManager()
        {
        }

        public void Initialise()
        {
            InitialiseScripts();
            log.Info($"Loaded {creatureScripts.Count} creature scripts.");
        }

        private void InitialiseScripts()
        {
            var creatureDict = ImmutableDictionary.CreateBuilder<uint, CreatureScript>();
            var plugDict = ImmutableDictionary.CreateBuilder<uint, PlugScript>();
            var questDict = ImmutableDictionary.CreateBuilder<uint, QuestScript>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(ScriptAttribute), true).Length > 0))
            {
                object instance = Activator.CreateInstance(type);
                foreach (ScriptAttribute attribute in type.GetCustomAttributes<ScriptAttribute>())
                {
                    if (type.IsSubclassOf(typeof(CreatureScript)))
                        creatureDict.TryAdd(attribute.Id, instance as CreatureScript);

                    if (type.IsSubclassOf(typeof(PlugScript)))
                        plugDict.TryAdd(attribute.Id, instance as PlugScript);

                    if (type.IsSubclassOf(typeof(QuestScript)))
                        questDict.TryAdd(attribute.Id, instance as QuestScript);
                }
            }

            creatureScripts = creatureDict.ToImmutable();
            plugScripts = plugDict.ToImmutable();
            questScripts = questDict.ToImmutable();
        }

        public T GetScript<T>(uint id) where T : Script
        {
            if (typeof(CreatureScript).IsAssignableFrom(typeof(T)))
                return creatureScripts.TryGetValue(id, out CreatureScript creatureScript) ? creatureScript as T : null;

            if (typeof(PlugScript).IsAssignableFrom(typeof(T)))
                return plugScripts.TryGetValue(id, out PlugScript plugScript) ? plugScript as T : null;

            if (typeof(QuestScript).IsAssignableFrom(typeof(T)))
                return questScripts.TryGetValue(id, out QuestScript questScript) ? questScript as T : null;

            log.Warn($"Unhandled ScriptType {typeof(T)}");
            return null;
        }
    }
}
