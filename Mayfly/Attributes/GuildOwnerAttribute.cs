using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Mayfly.Attributes
{
	public class GuildOwnerAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (context.User.Id == context.Guild.OwnerId)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			
			return Task.FromResult(PreconditionResult.FromError("Invoked by unauthorized user."));
		}
	}
}