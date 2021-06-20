using NexusForever.Database.Character.Model;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Game.Social;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Linq;

namespace NexusForever.WorldServer.Game.Guild
{
    public class Community : GuildBase
    {
        public override uint MaxMembers => 20u;

        /// <summary>
        /// Create a new <see cref="Community"/> using <see cref="GuildModel"/>
        /// </summary>
        public Community(GuildModel baseModel) 
            : base(baseModel)
        {
        }

        /// <summary>
        /// Create a new <see cref="Community"/> using the supplied parameters.
        /// </summary>
        public Community(string name, string leaderRankName, string councilRankName, string memberRankName)
            : base(GuildType.Community, name, leaderRankName, councilRankName, memberRankName)
        {
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
