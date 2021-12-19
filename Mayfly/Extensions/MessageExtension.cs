using System;
using System.Linq;
using Discord;

namespace Mayfly.Extensions
{
	public static class MessageExtension
	{
		public static bool Mentions(this IMessage message, IUser user)
		{
			return message.MentionedUserIds.Contains(user.Id);
		}

		public static bool Mentions(this IMessage message, IGuildChannel channel)
		{
			return message.MentionedChannelIds.Contains(channel.Id);
		}

		public static bool Mentions(this IMessage message, IRole role)
		{
			return message.MentionedRoleIds.Contains(role.Id);
		}

		public static bool IsDeletable(this IMessage message)
		{
			return (DateTimeOffset.UtcNow - message.Timestamp).TotalDays <= 14;
		}
	}
}