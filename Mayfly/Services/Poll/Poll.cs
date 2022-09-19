using Discord;
using Discord.Interactions;

namespace Mayfly.Services.Poll
{
	public class Poll
	{
		private static readonly string[] OptionChars = new string[20]
		{
			"🇦", "🇧", "🇨", "🇩", "🇪",
			"🇫", "🇬", "🇭", "🇮", "🇯",
			"🇰", "🇱", "🇲", "🇳", "🇴",
			"🇵", "🇶", "🇷", "🇸", "🇹"
		};
		
		public string Title { get; }
		public int TotalVotes { get; set; }
		public bool Finished { get; private set; }
		public DateTimeOffset EndTime { get; }
		
		private int lastVotes;
		private DateTime time = DateTime.Now;
		private Task rateLimitTask = Task.CompletedTask;
		private CancellationTokenSource timeoutToken = new CancellationTokenSource();
		
		public HashSet<ulong> Voters { get; } = new HashSet<ulong>();
		public Dictionary<IEmote, PollOption> Options { get; } = new Dictionary<IEmote, PollOption>();
		public IUserMessage Message { get; private set; }

		public Poll(string title, DateTimeOffset timeout, string[] args)
		{
			if (2 > args.Length || args.Length > OptionChars.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(args));
			}
			
			Title = title;
			EndTime = timeout;
			
			for (int i = 0; i < args.Length; i++)
			{
				Options.Add(new Emoji(OptionChars[i]), new PollOption(args[i]));
			}
		}

		private MessageComponent BuildButtons()
		{
			ComponentBuilder builder = new ComponentBuilder();

			for (int i = 0; i < Options.Count; i++)
			{
				builder.WithButton(emote: new Emoji(OptionChars[i]), customId: OptionChars[i], style: ButtonStyle.Secondary);
			}

			return builder.Build();
		}

		private Embed Build()
		{
			return new EmbedBuilder()
			{
				Title = Title,
				Color = Color.Teal,

				Fields = Options.Select(x => new EmbedFieldBuilder()
				{
					Name = $"{x.Key.Name} {x.Value.Name} ({x.Value.Percentage(TotalVotes)})",
					Value = x.Value.Bar(TotalVotes)
				}).ToList(),
				
				Footer = new EmbedFooterBuilder()
				{
					Text = $"{TotalVotes} votes"
				}
			}.Build();
		}

		public async Task Setup(SocketInteractionContext ctx)
		{
			await ctx.Interaction.RespondAsync($"Poll ends <t:{EndTime.ToUnixTimeSeconds()}:R>", embed: Build(), components: BuildButtons());
			Message = await ctx.Interaction.GetOriginalResponseAsync();
		}

		public async Task Update()
		{
			if (Finished)
			{
				return;
			}
			
			if (time >= DateTime.Now)
			{
				if (rateLimitTask is not { Status: TaskStatus.Running })
				{
					rateLimitTask = Task.Delay(DateTime.Now.Subtract(time)).ContinueWith(async _ =>
					{
						try
						{
							await Update();
						}
						catch
						{
							Console.WriteLine("Lmfao this shouldn't have happened but a delayed Update call for a Poll just failed to do it's thing.");
						}
					});
				}

				return;
			}
			
			// Just in case some timing issues happen. This should never be true but you never know.
			if (lastVotes == TotalVotes)
			{
				return;
			}
			
			lastVotes = TotalVotes;
			time = DateTime.Now.AddSeconds(2);
			await Message.ModifyAsync(x => x.Embed = Build());
		}

		public async Task<bool> Timeout(TimeSpan span)
		{
			try
			{
				await Task.Delay(span, timeoutToken.Token);
				return true;
			}
			catch { /* Ignore */ }

			return false;
		}

		public void Cancel()
		{
			using (timeoutToken)
			{
				timeoutToken.Cancel();
			}
		}
		
		public async Task Finish()
		{
			Finished = true;
			
			await Message.ModifyAsync(x =>
			{
				x.Content = $"Poll ended <t:{EndTime.ToUnixTimeSeconds()}:R>";
				x.Embed = Build();
				x.Components = null;
			});
		}
	}
}