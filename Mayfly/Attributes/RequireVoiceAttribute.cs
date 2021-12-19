using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Mayfly.Attributes
{
	public class RequireVoiceAttribute : PreconditionAttribute
	{
		public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (context.User is not IGuildUser user || user.VoiceChannel == null)
			{
				return PreconditionResult.FromError("User not in voice channel.");
			}

			ChannelPermissions perms = (await context.Guild.GetCurrentUserAsync()).GetPermissions(user.VoiceChannel);

			if (perms is { Connect: false } or { Speak: false } or { ViewChannel: false })
			{
				return PreconditionResult.FromError("Don't have permission to connect to that voice channel.");
			}

			return PreconditionResult.FromSuccess();
		}
	}
}