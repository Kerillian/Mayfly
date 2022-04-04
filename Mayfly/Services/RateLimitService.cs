using Discord;
using Discord.Interactions;

namespace Mayfly.Services
{
	public class RateLimitService
	{
		private readonly Dictionary<(ulong, string), DateTime> rateLimits = new Dictionary<(ulong, string), DateTime>();

		public bool IsLimited(IInteractionContext context, ICommandInfo info, out TimeSpan timeLeft)
		{
			(ulong Id, string Name) pair = (context.User.Id, info.Name);
			
			if (rateLimits.TryGetValue(pair, out DateTime date))
			{
				if (date > DateTime.UtcNow)
				{
					timeLeft = date - DateTime.UtcNow;
					return true;
				}

				rateLimits.Remove(pair);
			}
			
			timeLeft = TimeSpan.Zero;
			return false;
		}

		public bool SetLimited(IInteractionContext context, ICommandInfo info, TimeSpan time)
		{
			return rateLimits.TryAdd((context.User.Id, info.Name), DateTime.UtcNow + time);
		}
	}
}