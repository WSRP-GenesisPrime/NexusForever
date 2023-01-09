using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Map;
using System.Collections.Generic;
using System.Numerics;

namespace NexusForever.WorldServer.Game.Entity
{
    public class PendingTeleport
    {
        public TeleportReason Reason { get; init; }
        public MapPosition MapPosition { get; init; }
        public uint? VanityPetId { get; private set; }
        public List<Pet> Pets { get; } = new List<Pet>();

        public void AddPet(Pet pet)
        {
            Pets.Add(pet);
        }

        public void AddVanityPet(uint? petGuid)
        {
            VanityPetId = petGuid;
        }
    }
}
