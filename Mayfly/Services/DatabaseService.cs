using System.Numerics;
using Discord;
using Mayfly.Database;
using Microsoft.EntityFrameworkCore;

namespace Mayfly.Services
{
	public enum MarketCode
	{
		Success,
		Air,
		TooExpensive,
		UserNotFound,
		ItemNotFound,
	}
	
	public readonly record struct TransactionResponse(MarketCode Code, int Amount, int Money);

	public class DatabaseService
	{
		public readonly MayflyContext Context;

		public DatabaseService(MayflyContext mc)
		{
			this.Context = mc;
		}
		
		/*
		 * Static helper methods
		 */
		
		// a\ \cdot\ \log\left(\frac{x}{b}+1\right)
		public static int CalculateLevel(ulong xp)
		{
			return (int)MathF.Floor(234f * MathF.Log10(xp / 2985f + 1f));
		}

		/*
		 * User methods
		 */

		public async Task<UserData> GetUserAsync(IUser user)
		{
			return await Context.Users.FindAsync(user.Id);
		}

		public async Task ModifyUserAsync(IUser user, Action<UserData> action)
		{
			UserData data = await Context.Users.FindAsync(user.Id);
			bool isNull = data == null;
			
			if (isNull)
			{
				data = new UserData()
				{
					UserId = user.Id,
					Invokes = 0,
					Experience = 0,
					Money = 0
				};
			}

			action(data);

			if (isNull)
			{
				Context.Users.Add(data);
			}
			
			await Context.SaveChangesAsync();
		}

		public async Task<string> TotalMoney()
		{
			BigInteger big = 0;
			return (await Context.Users.ToArrayAsync()).Aggregate(big, (current, user) => BigInteger.Add(current, user.Money)).ToString("N0");
		}

		public async Task Save()
		{
			await Context.SaveChangesAsync();
		}
	}
}