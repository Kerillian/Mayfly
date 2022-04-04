using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Mayfly.Database;
using Mayfly.Services;

namespace Mayfly.Modules
{
	[Group("root", "Bot developer commands."), RequireOwner, DontAutoRegister]
	public class RootModule : MayflyModule
	{
		public DatabaseService Database { get; set; }
		
		[SlashCommand("resetnick", "Resets personal nickname across all guilds.")]
		public async Task ResetNickname()
		{
			foreach (SocketGuild guild in Context.Client.Guilds)
			{
				if (guild.CurrentUser.GuildPermissions.ChangeNickname)
				{
					await guild.CurrentUser.ModifyAsync(u => u.Nickname = string.Empty);
				}
			}

			await RespondAsync("Reset nicknames.", ephemeral: true);
		}
		
		[SlashCommand("leave", "Leave guild.")]
		public async Task Leave(ulong id)
		{
			SocketGuild guild = Context.Client.GetGuild(id);

			if (guild != null)
			{
				await guild.LeaveAsync();
				await RespondAsync("Left the guild.", ephemeral: true);
			}
		}

		[SlashCommand("guilds", "List joined guilds.")]
		public async Task Guilds()
		{
			StringBuilder builder = new StringBuilder();

			foreach (SocketGuild guild in Context.Client.Guilds)
			{
				builder.AppendLine($"{guild.Id} - {guild.Name}");
			}

			await RespondWithCodeAsync(builder.ToString(), ephemeral: true);
		}

		[SlashCommand("invite", "Generate invite for guild.")]
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
						await RespondAsync(invite.Url, ephemeral: true);
						return;
					}
				}

				await RespondAsync("Failed to find a valid channel for making an invite.", ephemeral: true);
			}
		}

		[SlashCommand("announce", "Broadcast an announcement to all guilds.")]
		public async Task Announce(string text)
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

				await RespondAsync("Sent announcement.", ephemeral: true);
			}
		}

		[Group("db", "Database stuff."), RequireOwner, DontAutoRegister]
		public class DatabaseModule : MayflyModule
		{
			public DatabaseService Database { get; set; }
			
			[SlashCommand("get", "Get raw database info for user.")]
			public async Task Get(IUser user)
			{
				UserData data = await Database.GetUserAsync(user);
				
				if (data is not null)
				{
					await RespondWithCodeAsync($"Id: {data.UserId}\nXP: {data.Experience}\nInvokes: {data.Invokes}\nCash: {data.Money}\nTokens: {data.Tokens}\n", ephemeral: true);
				}
				else
				{
					await RespondAsync("No user found", ephemeral: true);
				}
			}

			[SlashCommand("setmoney", "Set users money.")]
			public async Task SetMoney(IUser user, int money)
			{
				await Database.ModifyUserAsync(user, data =>
				{
					data.Money = money;
				});

				await RespondAsync($"Set `{user.Username}:{user.Discriminator}` money to `${money}`", ephemeral: true);
			}
			
			[SlashCommand("settokens", "Set users tokens.")]
			public async Task SetTokens(IUser user, int tokens)
			{
				await Database.ModifyUserAsync(user, data =>
				{
					data.Tokens = tokens;
				});

				await RespondAsync($"Set `{user.Username}:{user.Discriminator}` tokens to `${tokens}`", ephemeral: true);
			}
		}
	}
}