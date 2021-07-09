namespace NexusForever.Database.Character.Model
{
    public class ResidenceNeighborModel
    {
        public ulong Id { get; set; }
        public ulong ResidenceId { get; set; }
        public ulong ContactId { get; set; }
        public bool IsRoommate { get; set; }

        public virtual ResidenceModel Residence { get; set; }
    }
}
