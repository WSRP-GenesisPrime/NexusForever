using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NexusForever.WorldServer.Game.Entity
{
    public class PetManager : IEnumerable<WorldEntity>
    {
        private Player owner;

        private List<uint> combatPetGuids = new List<uint>();
        private uint? vanityPetGuid;

        public PetManager(Player player)
        {
            owner = player;

            // TODO: Implement resummoning combat pets on new session.
        }

        public IEnumerable<uint> GetCombatPetGuids()
        {
            return combatPetGuids;
        }

        public IEnumerable<Pet> GetCombatPets()
        {
            foreach (uint guid in combatPetGuids)
            {
                Pet pet = owner.GetVisible<Pet>(guid);
                if (pet != null)
                    yield return pet;
            }
        }

        public VanityPet GetVanityPet()
        {
            if (vanityPetGuid == null)
                return null;

            VanityPet pet = owner.GetVisible<VanityPet>(vanityPetGuid.Value);
            return pet;
        }

        public void SummonPet(PetType petType, uint creature, uint castingId, Spell4Entry spellInfo, Spell4EffectsEntry effectsEntry)
        {
            var position = new MapPosition
            {
                Position = owner.Position
            };

            switch (petType)
            {
                case PetType.CombatPet:
                    var pet = new Pet(owner, creature, castingId, spellInfo, effectsEntry);

                    position = new MapPosition 
                    {
                        Position = pet.GetSpawnPosition(owner)
                    };

                    if (owner.Map.CanEnter(pet, position))
                        owner.Map.EnqueueAdd(pet, position);
                    break;
                case PetType.VanityPet:
                    // enqueue removal of existing vanity pet if summoned
                    if (vanityPetGuid != null)
                    {
                        VanityPet oldVanityPet = owner.GetVisible<VanityPet>(vanityPetGuid.Value);
                        oldVanityPet?.RemoveFromMap();
                        vanityPetGuid = null;
                    }

                    var vanityPet = new VanityPet(owner, creature);
                    if (owner.Map.CanEnter(vanityPet, position))
                        owner.Map.EnqueueAdd(vanityPet, position);
                    break;
                default:
                    break;
            }
        }

        public void AddPetGuid(PetType petType, uint guid)
        {
            switch (petType)
            {
                case PetType.CombatPet:
                    combatPetGuids.Add(guid);
                    break;
                case PetType.VanityPet:
                    if (vanityPetGuid != null)
                        throw new InvalidOperationException();

                    vanityPetGuid = guid;
                    break;
                default:
                    break;
            }
        }

        public void RemovePetGuid(PetType petType, WorldEntity entity)
        {
            switch (petType)
            {
                case PetType.CombatPet:
                    combatPetGuids.Remove(entity.Guid);
                    break;
                case PetType.VanityPet:
                    if (vanityPetGuid == null)
                        throw new InvalidOperationException();

                    vanityPetGuid = null;
                    break;
                default:
                    break;
            }
        }

        public void OnTeleport(PendingTeleport pendingTeleport)
        {
            // store vanity pet summoned before teleport so it can be summoned again after being added to the new map
            uint? vanityPetId = null;
            if (vanityPetGuid != null)
            {
                VanityPet pet = owner.GetVisible<VanityPet>(vanityPetGuid.Value);
                vanityPetId = pet?.Creature.Id;

                if (vanityPetId != null)
                {
                    pendingTeleport.AddVanityPet(vanityPetId);
                    pet.RemoveFromMap();
                }
            }

            foreach (uint guid in combatPetGuids)
            {
                Pet pet = owner.GetVisible<Pet>(guid);
                if (pet == null)
                    throw new InvalidOperationException();

                pendingTeleport.AddPet(pet);
                pet.RemoveFromMap();
            }
            combatPetGuids.Clear();
        }

        public void OnAddToMap(PendingTeleport pendingTeleport)
        {
            if (pendingTeleport == null)
                return;

            // resummon vanity pet if it existed before teleport
            if (pendingTeleport?.VanityPetId != null)
            {
                var vanityPet = new VanityPet(owner, pendingTeleport.VanityPetId.Value);
                var position = new MapPosition
                {
                    Position = owner.Position
                };

                if (owner.Map.CanEnter(vanityPet, position))
                    owner.Map.EnqueueAdd(vanityPet, position);
            }

            if (pendingTeleport != null)
                foreach (Pet pet in pendingTeleport.Pets)
                {
                    pet.SetOwnerGuid(owner.Guid);
                    var position = new MapPosition
                    {
                        Position = pet.GetSpawnPosition(owner)
                    };
                    owner.Map.EnqueueAdd(pet, position);
                }
        }

        public void OnRemoveFromMap()
        {
            // enqueue removal of existing vanity pet if summoned
            if (vanityPetGuid != null)
            {
                VanityPet pet = owner.GetVisible<VanityPet>(vanityPetGuid.Value);
                pet?.RemoveFromMap();
                vanityPetGuid = null;
            }
        }

        public void DismissPets()
        {
            List<Pet> pets = GetCombatPets().ToList();
            if (pets.Count == 0)
                return;

            pets.Reverse();
            foreach (Pet pet in pets)
                pet.RemoveFromMap();
        }

        public void ApplyThreat(UnitEntity target)
        {
            List<Pet> pets = GetCombatPets().ToList();
            if (pets.Count == 0)
                return;

            foreach (Pet pet in pets)
                pet.ThreatManager.AddThreat(target, 1);
        }

        /// <summary>
        /// Update the stats of all Pets. To be used after equipment change or level up.
        /// </summary>
        public void UpdateStats()
        {
            foreach (Pet pet in GetCombatPets())
                pet.UpdateStats(owner);
        }

        public IEnumerator<WorldEntity> GetEnumerator()
        {
            List<WorldEntity> worldEntities = new List<WorldEntity>();

            foreach (Pet pet in GetCombatPets())
                worldEntities.Add(pet as WorldEntity);

            worldEntities.Add(GetVanityPet() as WorldEntity);

            return worldEntities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
