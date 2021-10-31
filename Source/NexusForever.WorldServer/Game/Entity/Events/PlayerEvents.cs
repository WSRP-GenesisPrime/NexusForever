using NexusForever.Shared.Game.Events;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Static;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Game.Quest.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Numerics;

namespace NexusForever.WorldServer.Game.Entity
{
    public partial class Player
    {
        private float perSecondTick;
        private double medicTickTimer;
        private float medicRefillRate = 4f;
        private float spellPowerRefillRate = 4f;
        private double warriorBuilderTimer;
        private float warriorDecayRate = 150f;
        private float esperResetTimer;

        private void OnLogin()
        {
            string motd = WorldServer.RealmMotd;
            if (motd?.Length > 0)
                GlobalChatManager.Instance.SendMessage(Session, motd, "MOTD", ChatChannelType.Realm);

            GuildManager.OnLogin();
            ChatManager.OnLogin();
            ContactManager.OnLogin();
            ShutdownManager.Instance.OnLogin(this);
        }

        private void OnLogout()
        {
            GuildManager.OnLogout();
            ChatManager.OnLogout();
            GlobalChatManager.Instance.LeaveDefaultChatChannels(this);
        }

        public override void OnAddToMap(BaseMap map, uint guid, Vector3 vector)
        {
            IsLoading = true;

            Session.EnqueueMessageEncrypted(new ServerChangeWorld
            {
                WorldId  = (ushort)map.Entry.Id,
                Position = new Position(vector),
                Yaw      = Rotation.X
            });

            // this must come before OnAddToMap
            // the client UI initialises the Holomark checkboxes during OnDocumentReady
            SendCharacterFlagsUpdated();

            base.OnAddToMap(map, guid, vector);

            // resummon vanity pet if it existed before teleport
            if (pendingTeleport?.VanityPetId != null)
            {
                var vanityPet = new VanityPet(this, pendingTeleport.VanityPetId.Value);

                var position = new MapPosition
                {
                    Position = Position
                };

                if (map.CanEnter(vanityPet, position))
                    map.EnqueueAdd(vanityPet, position);
            }

            pendingTeleport = null;

            GlobalChatManager.Instance.JoinDefaultChatChannels(this);
            SendPacketsAfterAddToMap();

            if (Health == 0u)
                SetDeathState(DeathState.JustDied);
            else
                SetDeathState(DeathState.JustSpawned);

            if (PreviousMap == null)
                OnLogin();
        }

        public override void OnRemoveFromMap()
        {
            DestroyDependents();

            base.OnRemoveFromMap();
        }


        public override void OnRelocate(Vector3 vector)
        {
            base.OnRelocate(vector);
            saveMask |= PlayerSaveMask.Location;

            ZoneMapManager.OnRelocate(vector);
        }

        protected override void OnZoneUpdate()
        {
            if (Zone != null)
            {
                TextTable tt = GameTableManager.Instance.GetTextTable(Language.English);
                if (tt != null)
                {
                    GlobalChatManager.Instance.SendMessage(Session, $"New Zone: ({Zone.Id}){tt.GetEntry(Zone.LocalizedTextIdName)}");
                }

                uint tutorialId = AssetManager.Instance.GetTutorialIdForZone(Zone.Id);
                if (tutorialId > 0)
                {
                    Session.EnqueueMessageEncrypted(new ServerTutorial
                    {
                        TutorialId = tutorialId
                    });
                }

                QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterZone, Zone.Id, 1);
            }

            ZoneMapManager.OnZoneUpdate();
        }

        /// <summary>
        /// Fires every time a regeneration tick occurs (every 0.5s)
        /// </summary>
        protected override void OnTickRegeneration()
        {
            base.OnTickRegeneration();

            perSecondTick += 0.5f;

            float resource3Remaining = GetStatFloat(Stat.Resource3) ?? 0f;
            if (Class == Class.Stalker && resource3Remaining < GetPropertyValue(Property.ResourceMax3))
            {
                float resource3RegenAmount = GetPropertyValue(Property.ResourceMax3) * GetPropertyValue(Property.ResourceRegenMultiplier3);
                SetStat(Stat.Resource3, (float)Math.Min(resource3Remaining + resource3RegenAmount, (float)GetPropertyValue(Property.ResourceMax3)));
            }

            float enduranceRemaining = GetStatFloat(Stat.Resource0) ?? 0f;
            if (enduranceRemaining < GetPropertyValue(Property.ResourceMax0))
            {
                float enduranceRegenAmount = GetPropertyValue(Property.ResourceMax0) * GetPropertyValue(Property.ResourceRegenMultiplier0);
                SetStat(Stat.Resource0, (float)Math.Min(enduranceRemaining + enduranceRegenAmount, (float)GetPropertyValue(Property.ResourceMax0)));
            }

            float dashRemaining = GetStatFloat(Stat.Dash) ?? 0f;
            if (dashRemaining < GetPropertyValue(Property.ResourceMax7))
            {
                float dashRegenAmount = GetPropertyValue(Property.ResourceMax7) * GetPropertyValue(Property.ResourceRegenMultiplier7);
                SetStat(Stat.Dash, (float)Math.Min(dashRemaining + dashRegenAmount, (float)GetPropertyValue(Property.ResourceMax7)));
            }

            float focusRemaining = GetStatFloat(Stat.Focus) ?? 0f;
            if (perSecondTick >= 1f && focusRemaining < GetPropertyValue(Property.BaseFocusPool))
            {
                float focusRegenAmount = GetPropertyValue(Property.BaseFocusPool) * (InCombat ? GetPropertyValue(Property.BaseFocusRecoveryInCombat) : GetPropertyValue(Property.BaseFocusRecoveryOutofCombat));
                Focus += focusRegenAmount;
            }

            switch (Class)
            {
                case Class.Warrior:
                    if (InCombat)
                    {
                        warriorBuilderTimer += 0.5f;
                        if (warriorBuilderTimer > 3d)
                        {
                            if (Resource1 > 0f)
                                Resource1 -= (float)(warriorDecayRate * 0.5f);

                            if (Resource1 < float.Epsilon)
                                warriorBuilderTimer = 0f;
                        }
                    }
                    else
                    {
                        if (Resource1 > 0f)
                            Resource1 -= (float)(warriorDecayRate * 0.5f);
                    }
                    break;
                case Class.Spellslinger:
                    if (Resource4 < GetPropertyValue(Property.ResourceMax4))
                        Resource4 += (float)(spellPowerRefillRate * 0.5f);
                    break;
                case Class.Medic:
                    if (!InCombat)
                    {
                        medicTickTimer += 0.5f;
                        if (medicTickTimer > 1d)
                        {
                            if (Resource1 < GetPropertyValue(Property.ResourceMax1))
                                Resource1 += medicRefillRate;
                            medicTickTimer = 0d;
                        }
                    }
                    break;
                case Class.Esper:
                    if (!InCombat)
                    {
                        esperResetTimer += 0.5f;
                        if (esperResetTimer >= 10d)
                        {
                            Resource1 = 0f;
                            esperResetTimer = 0f;
                        }
                    }
                    else
                    {
                        if (esperResetTimer > 0f)
                            esperResetTimer = 0f;
                    }
                    break;
            }

            if (perSecondTick >= 1f)
                perSecondTick = 0f;
        }

        protected override void OnStatChange(Stat stat, float newVal, float previousVal)
        {
            base.OnStatChange(stat, newVal, previousVal);

            switch (stat)
            {
                case Stat.Resource1:
                    if (Class == Class.Warrior && newVal >= previousVal)
                        warriorBuilderTimer = 0f;
                    break;
            }
        }

        protected override void OnDeath(UnitEntity killer)
        {
            base.OnDeath(killer);
        }
    }
}
