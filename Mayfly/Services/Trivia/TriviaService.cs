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
			this.discord.MessageDeleted += OnMessageDeleted;
		}

		private Task OnMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
		{
			foreach (KeyValuePair<ulong,TriviaSession> pair in sessions)
			{
				if (pair.Value.IsMyMessage(message.Id))
				{
					pair.Value.ForceStop();
					break;
				}
			}

			return Task.CompletedTask;
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

				try
				{
					await session.Setup(ctx.Interaction, options);
					await session.StartTimeout();
				}
				catch { /* ignore */ }

				this.sessions.TryRemove(ctx.Guild.Id, out session);
				return true;
			}

			return false;
		}
	}
}