using NexusForever.Database.Character.Model;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Game.Housing;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Linq;

namespace NexusForever.WorldServer.Game.Guild
{
    public partial class Community : GuildChat
    {
        public override uint MaxMembers => 20u;

        public Residence Residence { get; set; }

        /// <summary>
        /// Create a new <see cref="Community"/> using <see cref="GuildModel"/>
        /// </summary>
        public Community(GuildModel baseModel) 
            : base(baseModel)
        {
            InitialiseChatChannels(ChatChannelType.Community, null);
        }

        /// <summary>
        /// Create a new <see cref="Community"/> using the supplied parameters.
        /// </summary>
        public Community(string name, string leaderRankName, string councilRankName, string memberRankName)
            : base(GuildType.Community, name, leaderRankName, councilRankName, memberRankName)
        {
            InitialiseChatChannels(ChatChannelType.Community, null);
        }

        /// <summary>
        /// Set <see cref="Community"/> privacy level.
        /// </summary>
        public void SetCommunityPrivate(bool enabled)
        {
            if (enabled)
                SetFlag(GuildFlag.CommunityPrivate);
            else
                RemoveFlag(GuildFlag.CommunityPrivate);

            SendGuildFlagUpdate();
        }

        /// <summary>
        /// Return a <see cref="GuildData"/> packet of this <see cref="Community"/>
        /// </summary>
        public override GuildData BuildGuildDataPacket()
        {
            return new GuildData
            {
                GuildId = Id,
                GuildName = Name,
                Type = Type,
                Ranks = GetGuildRanksPackets().ToList(),
                MemberCount = (uint)members.Count,
                OnlineMemberCount = (uint)onlineMembers.Count,
                GuildInfo =
                {
                    GuildCreationDateInDays = (float)DateTime.Now.Subtract(CreateTime).TotalDays * -1f
                }
            };
        }
    }
}
