using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

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
			
			this.Title = title;

			for (int i = 0; i < args.Length; i++)
			{
				this.Options.Add(new Emoji(OptionChars[i]), new PollOption(args[i]));
			}
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
			this.endTime = DateTimeOffset.Now.AddMinutes(10);
			//this.Message = .SendMessageAsync($"Poll ends <t:{endTime.ToUnixTimeSeconds()}:R>", false, this.Build());
			
			await ctx.Interaction.RespondAsync($"Poll ends <t:{endTime.ToUnixTimeSeconds()}:R>", embed: this.Build());
			this.Message = await ctx.Interaction.GetOriginalResponseAsync();

			await this.Message.AddReactionsAsync(this.Options.Keys.ToArray());
		}

		public async Task Update(bool ended = false)
		{
			if (this.Time > DateTime.Now)
			{
				if (!this.timeout)
				{
					Task _ = Task.Delay(DateTime.Now.Subtract(this.Time)).ContinueWith(async _t =>
					{
						await this.Message.ModifyAsync(x =>
						{
							x.Embed = this.Build();

							if (ended)
							{
								x.Content = $"Poll ended <t:{endTime.ToUnixTimeSeconds()}:R>";
							}
						});
						
						this.timeout = false;
					});
					
					this.timeout = true;
				}

				return;
			}

			if (this.lastVotes == this.TotalVotes)
			{
				return;
			}

			this.lastVotes = this.TotalVotes;
			this.Time = DateTime.Now + TimeSpan.FromSeconds(3);

			await this.Message.ModifyAsync(x =>
			{
				x.Embed = this.Build();
				
				if (ended)
				{
					x.Content = $"Poll ended <t:{endTime.ToUnixTimeSeconds()}:R>";
				}
			});
		}
		
		public async Task Finish()
		{
			await this.Message.RemoveAllReactionsAsync();
			await this.Update(true);
		}
	}
}