using Discord.WebSocket;

namespace Mayfly.Extensions
{
	public static class UserExtension
	{
		public static bool IsManageable(this SocketGuildUser user)
		{
			if (user.Id == user.Guild.OwnerId)
			{
				return false;
			}

			if (user.Id == user.Guild.CurrentUser.Id)
			{
				return false;
			}

			if (user.Guild.CurrentUser.Id == user.Guild.OwnerId)
			{
				return true;
			}

			return user.Guild.CurrentUser.Hierarchy > user.Hierarchy;
		}

		public static bool IsKickable(this SocketGuildUser user)
		{
			return IsManageable(user) && user.Guild.CurrentUser.GuildPermissions.KickMembers;
		}
		
		public static bool IsBannable(this SocketGuildUser user)
		{
			return IsManageable(user) && user.Guild.CurrentUser.GuildPermissions.BanMembers;
		}
	}
}