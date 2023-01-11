using System;
using System.Collections.Generic;

namespace NexusForever.Database.Auth.Model
{
    public class AccountLinkModel
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateTime { get; set; }

        public ICollection<AccountLinkEntryModel> AccountLinkEntry { get; set; } = new HashSet<AccountLinkEntryModel>();
    }
}