namespace NexusForever.Database.Auth.Model
{
    public class AccountLinkEntryModel
    {
        public string Id { get; set; }
        public uint AccountId { get; set; }

        public AccountLinkModel Link { get; set; }
        public AccountModel Account { get; set; }
    }
}