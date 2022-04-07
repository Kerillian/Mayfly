using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace Mayfly.Services.Trivia
{
	public class TriviaService
	{
		private readonly ConcurrentDictionary<ulong, TriviaSession> sessions = new ConcurrentDictionary<ulong, TriviaSession>();
		private readonly DiscordSocketClient discord;
		private readonly HttpService http;

		public TriviaService(DiscordSocketClient dsc, HttpService hh)
		{
			this.discord = dsc;
			this.http = hh;

			this.discord.ButtonExecuted += HandleButtons;
		}

		private async Task HandleButtons(SocketMessageComponent interaction)
		{
			if (interaction.Channel is SocketTextChannel text && this.sessions.TryGetValue(text.Guild.Id, out TriviaSession session))
			{
				await interaction.DeferAsync();
				session.HandleButtons(interaction);
			}
		}

		public async Task<bool> NewSession(SocketInteractionContext ctx, TriviaOptions options)
		{
			if (!this.sessions.ContainsKey(ctx.Guild.Id))
			{
				TriviaSession session = new TriviaSession(this.http, ctx.User.Id);
				this.sessions.TryAdd(ctx.Guild.Id, session);
				await session.Setup(ctx.Interaction, options);
				await session.StartTimeout();

				this.sessions.TryRemove(ctx.Guild.Id, out session);
				return true;
			}

			return false;
		}
	}
}