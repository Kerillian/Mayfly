using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
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

		public static Embed ItemEmbed(ItemData item)
		{
			return new EmbedBuilder()
			{
				Title = item.Name,
				Description = item.Description,
				Color = new Color(item.Color),
				ThumbnailUrl = $"https://i.imgur.com/{item.ImgurId}",
				
				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder()
					{
						Name = "Amount",
						Value = item.Amount.ToString("N0"),
						IsInline = true
					},
					new EmbedFieldBuilder()
					{
						Name = "Cost",
						Value = $"${item.Cost:N0}",
						IsInline = true
					}
				}
			}.Build();
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
		
		/*
		 * Guild methods
		 */

		public async Task<GuildData> GetGuildAsync(IGuild guild)
		{
			return await Context.Guilds.FindAsync(guild.Id);
		}
		
		public async Task ModifyGuildAsync(IGuild guild, Action<GuildData> action)
		{
			GuildData data = await Context.Guilds.FindAsync(guild.Id);
			bool isNull = data == null;
			
			if (isNull)
			{
				data = new GuildData()
				{
					GuildId = guild.Id,
					AnnouncementId = 0,
					BlockAnnouncement = false
				};
			}

			action(data);

			if (isNull)
			{
				Context.Guilds.Add(data);
			}
			
			await Context.SaveChangesAsync();
		}

		/*
		 * Item methods
		 */
		
		private async Task AddItemAsync(ulong userId, string name, string description, string imgurId, uint color, uint cost, int amount)
		{
			if (1 > amount)
			{
				return;
			}
			
			ItemData item = await Context.Items.FirstOrDefaultAsync(i => i.UserId == userId && i.Name == name);

			if (item is null)
			{
				Context.Items.Add(new ItemData()
				{
					UserId = userId,
					Name = name,
					Description = description,
					ImgurId = imgurId,
					Color = color,
					Cost = cost,
					Amount = amount
				});
			}
			else
			{
				int sum = item.Amount + amount;
				
				if (1 > sum)
				{
					Context.Items.Remove(item);
				}
			}

			await Context.SaveChangesAsync();
		}

		public async Task AddItemAsync(string name, string description, string imageUrl, uint color, uint cost, int amount)
		{
			await AddItemAsync(ulong.MinValue, name, description, imageUrl, color, cost, amount);
		}
		
		public async Task AddItemAsync(IUser user, string name, string description, string imageUrl, uint color, uint cost, int amount)
		{
			await AddItemAsync(user.Id, name, description, imageUrl, color, cost, amount);
		}

		public async Task DeleteItemAsync(IUser user, string name)
		{
			Context.Items.RemoveRange(Context.Items.Where(i => i.UserId == user.Id && i.Name == name));
			await Context.SaveChangesAsync();
		}
		
		public async Task DeleteItemAsync(string name)
		{
			Context.Items.RemoveRange(Context.Items.Where(i => i.UserId == ulong.MinValue && i.Name == name));
			await Context.SaveChangesAsync();
		}
		
		public async Task<List<ItemData>> GetItemsAsync(IUser user)
		{
			return await Context.Items.Where(i => i.UserId == user.Id).ToListAsync();
		}

		public async Task<List<ItemData>> GetItemsAsync()
		{
			return await Context.Items.Where(i => i.UserId == ulong.MinValue).ToListAsync();
		}

		public async Task<TransactionResponse> SellItemAsync(IUser user, string name, int amount)
		{
			if (1 > amount)
			{
				return new TransactionResponse(MarketCode.Air, 0, 0);
			}

			UserData seller = await Context.Users.FindAsync(user.Id);

			if (seller is null)
			{
				return new TransactionResponse(MarketCode.UserNotFound, 0, 0);
			}
			
			ItemData item = await Context.Items.FirstOrDefaultAsync(i => i.UserId == user.Id && i.Name == name);

			if (item is null)
			{
				return new TransactionResponse(MarketCode.ItemNotFound, 0, 0);
			}
			
			amount = Math.Min(amount, item.Amount);
			int cash = (int)item.Cost * amount;
			seller.Money += cash;

			if (amount == item.Amount)
			{
				item.UserId = ulong.MinValue;
				await Context.SaveChangesAsync();
			}
			else
			{
				item.Amount -= amount;
				await AddItemAsync(user, item.Name, item.Description, item.ImgurId, item.Color, item.Cost, amount);
			}

			return new TransactionResponse(MarketCode.Success, amount, cash);
		}

		public async Task<TransactionResponse> PurchaseItemAsync(IUser user, string name, int amount)
		{
			if (1 > amount)
			{
				return new TransactionResponse(MarketCode.Air, 0, 0);
			}

			UserData buyer = await Context.Users.FindAsync(user.Id);

			if (buyer is null)
			{
				return new TransactionResponse(MarketCode.UserNotFound, 0, 0);
			}
			
			ItemData item = await Context.Items.FirstOrDefaultAsync(i => i.UserId == ulong.MinValue && i.Name == name);

			if (item is null)
			{
				return new TransactionResponse(MarketCode.ItemNotFound, 0, 0);
			}
			
			amount = Math.Min(amount, item.Amount);
			int cost = (int)item.Cost * amount;

			if (cost > buyer.Money)
			{
				return new TransactionResponse(MarketCode.TooExpensive, amount, cost);
			}

			buyer.Money -= cost;

			if (amount == item.Amount)
			{
				item.UserId = user.Id;
				await Context.SaveChangesAsync();
			}
			else
			{
				item.Amount -= amount;
				await AddItemAsync(user, item.Name, item.Description, item.ImgurId, item.Color, item.Cost, amount);
			}
			
			return new TransactionResponse(MarketCode.Success, amount, cost);
		}

		/*
		 * Reminder methods
		 */
		
		public async Task AddReminderAsync(IUser user, string message, TimeSpan time)
		{
			Context.Reminders.Add(new ReminderData()
			{
				UserId = user.Id,
				Message = message,
				DueDate = DateTime.UtcNow + time,
			});

			await Context.SaveChangesAsync();
		}

		public async Task RemoveReminderAsync(ReminderData data)
		{
			Context.Reminders.Remove(data);
			await Context.SaveChangesAsync();
		}
		
		public async Task<List<ReminderData>> GetRemindersAsync(IUser user)
		{
			return await Context.Reminders.Where(r => r.UserId == user.Id).ToListAsync();
		}
		
		public async Task Save()
		{
			await Context.SaveChangesAsync();
		}
	}
}