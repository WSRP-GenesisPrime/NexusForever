﻿using NexusForever.Database.Character.Model;
using NexusForever.WorldServer.Game.Guild.Static;
using NexusForever.WorldServer.Game.Social.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System;
using System.Linq;

namespace NexusForever.WorldServer.Game.Guild
{
    public class WarParty : GuildChat
    {
        public override uint MaxMembers => 30u;

        /// <summary>
        /// Create a new <see cref="WarParty"/> using <see cref="GuildModel"/>
        /// </summary>
        public WarParty(GuildModel baseModel) 
            : base(baseModel)
        {
            InitialiseChatChannels(ChatChannelType.WarParty, ChatChannelType.WarPartyOfficer);
        }

        /// <summary>
        /// Create a new <see cref="WarParty"/> using the supplied parameters.
        /// </summary>
        public WarParty(string name, string leaderRankName, string councilRankName, string memberRankName)
            : base(GuildType.WarParty, name, leaderRankName, councilRankName, memberRankName)
        {
            InitialiseChatChannels(ChatChannelType.WarParty, ChatChannelType.WarPartyOfficer);
        }

        /// <summary>
        /// Return a <see cref="GuildData"/> packet of this <see cref="WarParty"/>
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
