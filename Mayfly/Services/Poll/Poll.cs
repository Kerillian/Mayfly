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
		private int lastVotes;
		private bool timeout;
		
		private DateTimeOffset endTime = DateTimeOffset.Now.AddMinutes(10);
		//private readonly ComponentBuilder componentBuilder = new ComponentBuilder();
		private MessageComponent component;

		public HashSet<ulong> Voters { get; } = new HashSet<ulong>();
		public Dictionary<IEmote, PollOption> Options { get; } = new Dictionary<IEmote, PollOption>();
		public IUserMessage Message;
		public DateTime Time = DateTime.Now;

		public Poll(string title, string[] args)
		{
			if (2 > args.Length || args.Length > OptionChars.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(args));
			}
			
			Title = title;

			ComponentBuilder builder = new ComponentBuilder();
			
			for (int i = 0; i < args.Length; i++)
			{
				Options.Add(new Emoji(OptionChars[i]), new PollOption(args[i]));
				builder.WithButton(emote: new Emoji(OptionChars[i]), customId: OptionChars[i], style: ButtonStyle.Secondary);
			}

			component = builder.Build();
		}

		public Embed Build()
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
			endTime = DateTimeOffset.Now.AddMinutes(10);

			await ctx.Interaction.RespondAsync($"Poll ends <t:{endTime.ToUnixTimeSeconds()}:R>", embed: Build(), components: component);
			Message = await ctx.Interaction.GetOriginalResponseAsync();
		}

		public async Task Update(bool ended = false)
		{
			if (Time > DateTime.Now)
			{
				if (!timeout)
				{
					Task _ = Task.Delay(DateTime.Now.Subtract(Time)).ContinueWith(async _t =>
					{
						await Message.ModifyAsync(x =>
						{
							x.Embed = Build();

							if (ended)
							{
								x.Content = $"Poll ended <t:{endTime.ToUnixTimeSeconds()}:R>";
								x.Components = null;
							}
						});
						
						timeout = false;
					});
					
					timeout = true;
				}

				return;
			}

			if (lastVotes == TotalVotes)
			{
				return;
			}

			lastVotes = TotalVotes;
			Time = DateTime.Now + TimeSpan.FromSeconds(3);

			await Message.ModifyAsync(x =>
			{
				x.Embed = Build();
				
				if (ended)
				{
					x.Content = $"Poll ended <t:{endTime.ToUnixTimeSeconds()}:R>";
				}
			});
		}
		
		public async Task Finish()
		{
			await Update(true);
		}
	}
}