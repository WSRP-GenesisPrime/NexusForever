using NexusForever.Database.Character.Model;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Linq;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Circle : GuildChat
    {
        public override uint MaxMembers => 1000u;

        /// <summary>
        /// Create a new <see cref="Circle"/> using an existing database model.
        /// </summary>
        public Circle(GuildModel baseModel) 
            : base(baseModel)
        {
            InitialiseChatChannels(ChatChannelType.Society, null);
        }

        /// <summary>
        /// Create a new <see cref="Circle"/> using the supplied parameters.
        /// </summary>
        public Circle(string name, string leaderRankName, string councilRankName, string memberRankName)
            : base(GuildType.Circle, name, leaderRankName, councilRankName, memberRankName)
        {
            InitialiseChatChannels(ChatChannelType.Society, null);
        }

        /// <summary>
        /// Return a <see cref="GuildData"/> packet of this <see cref="Circle"/>
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
