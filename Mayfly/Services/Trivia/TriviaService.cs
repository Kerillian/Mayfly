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
			this.discord.ReactionAdded += this.HandleReactions;
		}

		public Task HandleReactions(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			if (!message.HasValue || !reaction.User.IsSpecified)
			{
				return Task.CompletedTask;
			}

			if (reaction.UserId == this.discord.CurrentUser.Id)
			{
				return Task.CompletedTask;
			}

			if (channel.Value is SocketTextChannel text && this.sessions.TryGetValue(text.Guild.Id, out TriviaSession session))
			{
				session.HandleReaction(message.Value, reaction);
			}

			return Task.CompletedTask;
		}

		public async Task<bool> NewSession(SocketInteractionContext ctx, TriviaOptions options)
		{
			if (!this.sessions.ContainsKey(ctx.Guild.Id))
			{
				TriviaSession session = new TriviaSession(this.http, ctx.Interaction, ctx.Channel, ctx.User.Id);
				this.sessions.TryAdd(ctx.Guild.Id, session);
				await session.Setup(options);
				await session.StartTimeout();

				this.sessions.TryRemove(ctx.Guild.Id, out session);
				return true;
			}

			return false;
		}
	}
}