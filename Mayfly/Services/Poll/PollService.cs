using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Mayfly.Services.Poll
{
	public class PollService
	{
		private static readonly TimeSpan Timeout = TimeSpan.FromHours(1);
		private readonly ConcurrentDictionary<ulong, Poll> pollData = new ConcurrentDictionary<ulong, Poll>();

		public PollService(DiscordSocketClient dsc)
		{
			dsc.ButtonExecuted += HandleButtons;
			dsc.MessageDeleted += OnMessageDeleted;
		}

		private Task OnMessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
		{
			foreach (KeyValuePair<ulong, Poll> pair in pollData)
			{
				if (pair.Value.Message.Id == message.Id)
				{
					pair.Value.Cancel();
					break;
				}
			}

			return Task.CompletedTask;
		}

		private async Task HandleButtons(SocketMessageComponent interaction)
		{
			if (pollData.TryGetValue(interaction.Message.Id, out Poll poll))
			{
				if (poll.Voters.Contains(interaction.User.Id))
				{
					await interaction.RespondAsync("Your vote has already been counted.", ephemeral: true);
					return;
				}
				
				await interaction.DeferAsync();
				
				if (poll.Options.TryGetValue(new Emoji(interaction.Data.CustomId), out PollOption option))
				{
					option.Votes++;
					poll.TotalVotes++;
					poll.Voters.Add(interaction.User.Id);
					await poll.Update();
				}
			}
		}

		public async Task<bool> Create(SocketInteractionContext ctx, string title, string[] args)
		{
			if (args.Length is < 2 or > 20)
			{
				return false;
			}
			
			Poll poll = new Poll(title, DateTimeOffset.Now.Add(Timeout), args);

			await poll.Setup(ctx);
			pollData.TryAdd(poll.Message.Id, poll);

			if (await poll.Timeout(Timeout))
			{
				await poll.Finish();
			}

			pollData.TryRemove(poll.Message.Id, out _);

			return true;
		}
	}
}