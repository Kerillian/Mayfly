using System;
using System.Collections.Generic;
using Discord.Commands;

namespace Mayfly.Services
{
	public class RateLimitService
	{
		private readonly Dictionary<(ulong, string), DateTime> rateLimits = new Dictionary<(ulong, string), DateTime>();

		public bool IsLimited(ICommandContext context, CommandInfo info, out TimeSpan timeLeft)
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

		public bool SetLimited(ICommandContext context, CommandInfo info, TimeSpan time)
		{
			return rateLimits.TryAdd((context.User.Id, info.Name), DateTime.UtcNow + time);
		}
	}
}