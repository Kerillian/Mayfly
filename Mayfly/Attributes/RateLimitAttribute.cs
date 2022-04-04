using Discord;
using Discord.Interactions;
using Mayfly.Services;

namespace Mayfly.Attributes
{
	public class RateLimitAttribute : PreconditionAttribute
	{
		private readonly TimeSpan timeSpan;

		public RateLimitAttribute(int seconds)
		{
			this.timeSpan = TimeSpan.FromSeconds(seconds);
		}

		public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo info, IServiceProvider services)
		{
			if (services.GetService(typeof(RateLimitService)) is not RateLimitService rateLimiter)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			
			if (rateLimiter.IsLimited(context, info, out TimeSpan left))
			{
				return Task.FromResult(PreconditionResult.FromError($"You're running that command too fast.\nPlease try again in `{left.TotalSeconds:0}` seconds."));
			}
			
			rateLimiter.SetLimited(context, info, timeSpan);
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}