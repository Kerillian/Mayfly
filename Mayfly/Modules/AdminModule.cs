using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Attributes.Parameter;
using Mayfly.Extensions;

namespace Mayfly.Modules
{
	public class AdminModule : MayflyModule
	{
		public BotConfig Config { get; set; }

		[RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
		[Command("cleanup"), Summary("Cleanup bot IO from channel."), RequireContext(ContextType.Guild)]
		public async Task Cleanup([Range(1, 100)] int amount = 25)
		{
			if (Context.Channel is ITextChannel channel)
			{
				IEnumerable<IMessage> messages = await channel.GetMessagesAsync(amount).FlattenAsync();
				messages = messages.Where(m => m.IsDeletable() && (m.Author.Id == Context.Client.CurrentUser.Id || m.Content.StartsWith(this.Config.Prefix)));

				await channel.DeleteMessagesAsync(messages);
			}
		}

		[RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
		[Command("bulk"), Summary("Bulk delete messages from channel."), RequireContext(ContextType.Guild)]
		public async Task Bulk([Range(1, 100)] int amount = 25)
		{
			if (Context.Channel is ITextChannel channel)
			{
				await channel.DeleteMessagesAsync((await channel.GetMessagesAsync(amount).FlattenAsync()).Where(m => m.IsDeletable()));
			}
		}

		[RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
		[Command("bulk"), Summary("Bulk delete messages from user.")]
		public async Task Bulk(IGuildUser user, [Range(1, 100)] int amount = 25)
		{
			if (Context.Channel is ITextChannel channel)
			{
				IEnumerable<IMessage> messages = await channel.GetMessagesAsync(amount).FlattenAsync();
				messages = messages.Where(m => m.IsDeletable() && m.Author.Id == user.Id);

				await channel.DeleteMessagesAsync(messages);
			}
		}

		[Command("ban"), Summary("Ban user by ID."), RequireContext(ContextType.Guild)]
		[RequireBotPermission(GuildPermission.BanMembers), RequireUserPermission(GuildPermission.BanMembers)]
		public async Task Ban(ulong id, string reason = null)
		{
			await Context.Guild.AddBanAsync(id, 0, reason);
		}
	}
}