using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Mayfly.Attributes;
using Mayfly.Database;
using Mayfly.Services;

namespace Mayfly.Modules
{
	[Group("root"), RequireOwner, Hidden]
	public class RootModule : MayflyModule
	{
		[Command("resetnick")]
		public async Task ResetNickname()
		{
			foreach (SocketGuild guild in Context.Client.Guilds)
			{
				if (guild.CurrentUser.GuildPermissions.ChangeNickname)
				{
					await guild.CurrentUser.ModifyAsync(u => u.Nickname = string.Empty);
				}
			}
		}
		
		[Command("leave")]
		public async Task Leave(ulong id)
		{
			SocketGuild guild = Context.Client.GetGuild(id);

			if (guild != null)
			{
				await guild.LeaveAsync();
			}
		}

		[Command("guilds")]
		public async Task Guilds()
		{
			StringBuilder builder = new StringBuilder();

			foreach (SocketGuild guild in Context.Client.Guilds)
			{
				builder.AppendLine($"{guild.Id} - {guild.Name}");
			}

			await ReplyCodeAsync(builder.ToString());
		}

		[Command("invite")]
		public async Task Invite(ulong id)
		{
			SocketGuild guild = Context.Client.GetGuild(id);
			
			if (guild != null)
			{
				foreach (SocketTextChannel channel in guild.TextChannels)
				{
					if (guild.CurrentUser.GetPermissions(channel).CreateInstantInvite)
					{
						IInviteMetadata invite = await channel.CreateInviteAsync(60, 1, false, false);
						await ReplyAsync(invite.Url);
						return;
					}
				}

				await ReplyAsync("Failed to find a valid channel for making an invite.");
			}
		}

		[Command("announce")]
		public async Task Announce([Remainder] string text)
		{
			foreach (SocketGuild guild in Context.Client.Guilds)
			{
				SocketTextChannel announceChannel = null;
				
				if (guild.CurrentUser.GetPermissions(guild.DefaultChannel).SendMessages)
				{
					announceChannel = guild.DefaultChannel;
				}
				else if (guild.CurrentUser.GetPermissions(guild.SystemChannel).SendMessages)
				{
					announceChannel = guild.SystemChannel;
				}
				else
				{
					foreach (SocketTextChannel channel in guild.TextChannels.OrderBy(c => c.Position))
					{
						if (guild.CurrentUser.GetPermissions(channel).SendMessages)
						{
							announceChannel = channel;
							break;
						}
					}
				}
			
				if (announceChannel != null)
				{
					await announceChannel.SendMessageAsync("", false, new EmbedBuilder()
					{
						Title = "Announcement",
						Color = new Color(0xB4DC7A),
						Description = text
					}.Build());
				}
			}
		}

		[Group("db"), RequireOwner, Hidden]
		public class DatabaseModule : MayflyModule
		{
			public DatabaseService Database { get; set; }
			
			[Command("get")]
			public async Task Get(IUser user)
			{
				UserData data = await Database.GetUserAsync(user);
				
				if (data is not null)
				{
					StringBuilder builder = new StringBuilder();
					builder.AppendLine($"Id: {data.UserId}\nXP: {data.Experience}\nInvokes: {data.Invokes}\nCash: {data.Money}\nTokens: {data.Tokens}\n");
					builder.AppendLine("\n===== Inventory =====");

					foreach (ItemData item in await Database.GetItemsAsync(user))
					{
						builder.AppendLine($"Name: {item.Name}\nDescription: {item.Description}\nImage: {item.ImgurId}\nCost: {item.Cost}\n");
					}
					
					await using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString()));
					await ReplyFileAsync(stream, $"{user.Id}.txt");
				}
				else
				{
					await ReplyAsync("No user found");
				}
			}
			
			[Command("market")]
			public async Task GetMarket()
			{
				StringBuilder builder = new StringBuilder();
				List<ItemData> items = await Database.GetItemsAsync();
				
				foreach (ItemData item in items.OrderBy(i => i.Amount))
				{
					builder.AppendLine($"Name        : {item.Name}");
					builder.AppendLine($"Description : {item.Description}");
					builder.AppendLine($"ImgurId     : {item.ImgurId}");
					builder.AppendLine($"Cost        : {item.Cost}");
					builder.AppendLine($"UserId      : {item.Name}");
					builder.AppendLine();
				}

				await using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(builder.ToString()));
				await ReplyFileAsync(stream, "items.txt");
			}

			[Command("setmoney")]
			public async Task SetMoney(IUser user, int money)
			{
				await Database.ModifyUserAsync(user, data =>
				{
					data.Money = money;
				});
			}

			[Command("additem")]
			public async Task AddItem(IUser user, string name, string description, string imgurId, uint color, uint cost, int amount)
			{
				await Database.AddItemAsync(user, name, description, imgurId, color, cost, amount);
			}
			
			[Command("deleteitem")]
			public async Task DeleteItem(IUser user, string name)
			{
				await Database.DeleteItemAsync(user, name);
			}
			
			[Command("addmarket")]
			public async Task AddMarket(string name, string description, string imgurId, uint color, uint cost, int amount)
			{
				await Database.AddItemAsync(name, description, imgurId, color, cost, amount);
			}

			[Command("deletemarket")]
			public async Task DeleteMarket(string name)
			{
				await Database.DeleteItemAsync(name);
			}
		}
	}
}