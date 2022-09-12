using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Mayfly.Extensions;

namespace Mayfly.Modules
{
	/*
	 * Due to the intent being enforced now, this requires my bot to use the intent and to be verified.
	 * I don't really *want* to use the intent so I guess these won't do anything now.
	 */
	
	[DontAutoRegister, RequireContext(ContextType.Guild)]
	public class AdminModule : MayflyModule
	{
		public BotConfig Config { get; set; }

		[RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
		 [SlashCommand("cleanup", "Cleanup bot IO from channel.")]
		 public async Task Cleanup([MinValue(2), MaxValue(100)] int amount = 25)
		 {
		 	int deleted = 0;
		 	
		 	if (Context.Channel is ITextChannel channel && Context.User is SocketGuildUser user)
		 	{
		 		IEnumerable<IMessage> messages = await channel.GetMessagesAsync(amount).FlattenAsync();
		
		 		messages = user.GetPermissions(channel).ManageMessages
		 			? messages.Where(m => m.IsDeletable() && m.Author.Id == Context.Client.CurrentUser.Id)
		 			: messages.Where(m => m.IsDeletable() && m is SocketUserMessage { Interaction: { } } msg && msg.Interaction.User.Id == Context.User.Id);
		
		 		deleted = messages.Count();
		 		await channel.DeleteMessagesAsync(messages);
		 	}
		
		 	await RespondAsync($"Deleted `{deleted}` messages.", ephemeral: true);
		 }

		 [RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
		 [SlashCommand("bulk", "Bulk delete messages from channel.")]
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
		
		 [RequireUserPermission(GuildPermission.ManageMessages), RequireBotPermission(GuildPermission.ManageMessages)]
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