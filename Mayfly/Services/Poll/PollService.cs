using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Mayfly.Services.Poll
{
	public class PollService
	{
		private readonly DiscordSocketClient discord;
		private readonly Dictionary<ulong, Poll> pollData = new Dictionary<ulong, Poll>();

		public PollService(DiscordSocketClient dsc)
		{
			this.discord = dsc;
			this.discord.ReactionAdded += this.HandleReactions;
		}

		public async Task HandleReactions(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			if (!message.HasValue || reaction.UserId == this.discord.CurrentUser.Id)
			{
				return;
			}

			if (this.pollData.TryGetValue(message.Id, out Poll poll) && !poll.Voters.Contains(reaction.UserId))
			{
				if (poll.Options.TryGetValue(reaction.Emote, out PollOption option))
				{
					option.Votes++;
					poll.TotalVotes++;
					poll.Voters.Add(reaction.UserId);
					await poll.Update();
				}
			}
		}

		public async Task<bool> Create(ISocketMessageChannel channel, string title, string[] args)
		{
			if (args.Length is < 2 or > 20)
			{
				return false;
			}
			
			Poll poll = new Poll(title, args);
			
			await poll.Setup(channel);
			this.pollData.Add(poll.Message.Id, poll);
			
			Task _ = Task.Delay(TimeSpan.FromMinutes(10)).ContinueWith(async _ =>
			{
				this.pollData.Remove(poll.Message.Id);
				await poll.Finish();
			});

			return true;
		}
	}
}