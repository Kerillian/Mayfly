using Discord;
using Discord.Interactions;
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
			//this.discord.ReactionAdded += this.HandleReactions;
			this.discord.ButtonExecuted += this.HandleButtons;
		}

		private async Task HandleButtons(SocketMessageComponent interaction)
		{
			if (this.pollData.TryGetValue(interaction.Message.Id, out Poll poll) && !poll.Voters.Contains(interaction.User.Id))
			{
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
			
			Poll poll = new Poll(title, args);
			
			await poll.Setup(ctx);
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