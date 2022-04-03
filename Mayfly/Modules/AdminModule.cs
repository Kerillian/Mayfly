using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mayfly.Attributes.Parameter;
using Mayfly.Extensions;

namespace Mayfly.Modules
{
	public class AdminModule : MayflyInteraction
	{
		public BotConfig Config { get; set; }

		[RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
		[SlashCommand("cleanup", "Cleanup bot IO from channel."), RequireContext(ContextType.Guild)]
		public async Task Cleanup([MinValue(2), MaxValue(100)] int amount = 25)
		{
			int deleted = 0;
			
			if (Context.Channel is ITextChannel channel)
			{
				IEnumerable<IMessage> messages = await channel.GetMessagesAsync(amount).FlattenAsync();
				messages = messages.Where(m => m.IsDeletable() && m.Author.Id == Context.Client.CurrentUser.Id);

				deleted = messages.Count();
				await channel.DeleteMessagesAsync(messages);
			}

			await RespondAsync($"Deleted `{deleted}` messages.", ephemeral: true);
		}

		[RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
		[SlashCommand("bulk", "Bulk delete messages from channel."), RequireContext(ContextType.Guild)]
		public async Task Bulk([MinValue(2), MaxValue(100)] int amount = 25)
		{
			int deleted = 0;
			
			if (Context.Channel is ITextChannel channel)
			{
				IEnumerable<IMessage> messages = (await channel.GetMessagesAsync(amount).FlattenAsync()).Where(m => m.IsDeletable());

				deleted = messages.Count();
				await channel.DeleteMessagesAsync(messages);
			}
			
			await RespondAsync($"Deleted `{deleted}` messages.", ephemeral: true);
		}

		[RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
		[SlashCommand("bulkuser", "Bulk delete messages from user.")]
		public async Task BulkUser(IGuildUser user, [MinValue(2), MaxValue(100)] int amount = 25)
		{
			int deleted = 0;
			
			if (Context.Channel is ITextChannel channel)
			{
				IEnumerable<IMessage> messages = await channel.GetMessagesAsync(amount).FlattenAsync();
				messages = messages.Where(m => m.IsDeletable() && m.Author.Id == user.Id);

				deleted = messages.Count();
				await channel.DeleteMessagesAsync(messages);
			}
			
			await RespondAsync($"Deleted `{deleted}` messages.", ephemeral: true);
		}
	}
}