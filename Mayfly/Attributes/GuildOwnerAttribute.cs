using Discord;
using Discord.Interactions;

namespace Mayfly.Attributes
{
	public class GuildOwnerAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
		{
			if (context.User.Id == context.Guild.OwnerId)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			
			return Task.FromResult(PreconditionResult.FromError("Invoked by unauthorized user."));
		}
	}
}