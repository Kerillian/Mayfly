using System;
using System.Threading.Tasks;
using Discord.Commands;
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
		
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (services.GetService(typeof(RateLimitService)) is not RateLimitService rateLimiter)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			if (rateLimiter.IsLimited(context, command, out TimeSpan left))
			{
				return Task.FromResult(PreconditionResult.FromError($"You're running that command too fast.\nPlease try again in `{left.TotalSeconds:0}` seconds."));
			}

			rateLimiter.SetLimited(context, command, timeSpan);
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}