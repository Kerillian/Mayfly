using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Mayfly.Services.Poll
{
	public class PollService
	{
		private readonly TimeSpan timeout = TimeSpan.FromMinutes(1);
		private readonly Dictionary<ulong, Poll> pollData = new Dictionary<ulong, Poll>();

		public PollService(DiscordSocketClient dsc)
		{
			dsc.ButtonExecuted += HandleButtons;
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
			
			Poll poll = new Poll(title, DateTimeOffset.Now.Add(timeout), args);

			await poll.Setup(ctx);
			pollData.Add(poll.Message.Id, poll);

			await Task.Delay(timeout);
			await poll.Finish();
			
			pollData.Remove(poll.Message.Id);

			return true;
		}
	}
}