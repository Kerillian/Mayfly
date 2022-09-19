using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;

namespace Mayfly.Services.Trivia
{
	public class TriviaService
	{
		private readonly ConcurrentDictionary<ulong, TriviaSession> sessions = new ConcurrentDictionary<ulong, TriviaSession>();
		private readonly HttpService http;

		public TriviaService(DiscordSocketClient dsc, HttpService hh)
		{
			http = hh;

			dsc.ButtonExecuted += HandleButtons;
			dsc.MessageDeleted += OnMessageDeleted;
		}

		private Task OnMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
		{
			ulong id = 0;
			
			foreach (KeyValuePair<ulong,TriviaSession> pair in sessions)
			{
				if (pair.Value.IsMyMessage(message.Id))
				{
					pair.Value.ForceStop();
					id = pair.Key;
					break;
				}
			}

			if (id != 0)
			{
				sessions.TryRemove(id, out _);
			}

			return Task.CompletedTask;
		}

		private async Task HandleButtons(SocketMessageComponent interaction)
		{
			if (interaction.Channel is SocketTextChannel text && sessions.TryGetValue(text.Guild.Id, out TriviaSession session))
			{
				await interaction.DeferAsync();
				session.HandleButtons(interaction);
			}
		}

		public async Task<bool> NewSession(SocketInteractionContext ctx, TriviaOptions options)
		{
			if (!sessions.ContainsKey(ctx.Guild.Id))
			{
				TriviaSession session = new TriviaSession(http, ctx.User.Id);
				sessions.TryAdd(ctx.Guild.Id, session);

				try
				{
					await session.Setup(ctx.Interaction, options);
					await session.StartTimeout();
				}
				catch { /* ignore */ }

				sessions.TryRemove(ctx.Guild.Id, out _);
				return true;
			}

			return false;
		}
	}
}