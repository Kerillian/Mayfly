using System.Web;
using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Mayfly.Structures;

namespace Mayfly.Services.Trivia
{
	public class TriviaSession
	{
		private readonly HttpService http;
		private ConcurrentDictionary<ulong, TriviaPlayer> Players = new ConcurrentDictionary<ulong, TriviaPlayer>();
		private SocketInteraction interaction;
		private readonly ISocketMessageChannel channel;
		private IUserMessage message;
		private bool waitingForPlayers = true;
		private readonly ulong host;
		private CancellationTokenSource cancelQuestionWait = new CancellationTokenSource();
		private CancellationTokenSource cancelJoinWait = new CancellationTokenSource();

		private readonly Embed presetEmbed = new EmbedBuilder()
		{
			Title = "Trivia Lobby",
			Description = "To play click the  icon below!\n\n_(If you are the host, you can click the ▶️ to start)_",
			Color = Color.Green,
		}.Build();

		private readonly IEmote[] TrueFalseEmotes = new IEmote[2]
		{
			new Emoji("✔️"),
			new Emoji("⭕")
		};

		private readonly IEmote[] MultiChoiceEmotes = new IEmote[4]
		{
			new Emoji("🇦"),
			new Emoji("🇧"),
			new Emoji("🇨"),
			new Emoji("🇩"),
		};

		private TriviaResult trivia;
		private TriviaQuestion question;
		private readonly Dictionary<IEmote, string> answerDict = new Dictionary<IEmote, string>();
		private int answered;

		public TriviaSession(HttpService hs, SocketInteraction interaction, ISocketMessageChannel channel, ulong host)
		{
			this.http = hs;
			this.interaction = interaction;
			this.channel = channel;
			this.host = host;
		}

		public async Task Setup(TriviaOptions options)
		{
			await this.interaction.RespondAsync(embed: presetEmbed);
			this.message = await this.interaction.GetOriginalResponseAsync();
			this.trivia = await this.http.GetJsonAsync<TriviaResult>(options.Build());
			
			await this.message.AddReactionAsync(new Emoji("➕"));
			await this.message.AddReactionAsync(new Emoji("▶️"));

			try
			{
				await Task.Delay(30000, this.cancelJoinWait.Token);
				this.cancelJoinWait = new CancellationTokenSource();
			}
			catch { /* Ignored */ }
		}

		public void CancelWait()
		{
			using (this.cancelQuestionWait)
			{
				this.cancelQuestionWait.Cancel();
			}

			this.cancelQuestionWait = new CancellationTokenSource();
		}

		public void HandleReaction(IUserMessage msg, SocketReaction reaction)
		{
			if (msg.Id != this.message.Id)
			{
				return;
			}

			if (this.waitingForPlayers)
			{
				switch (reaction.Emote.Name)
				{
					case "▶️":
					{
						if (reaction.UserId == this.host)
						{
							this.waitingForPlayers = false;

							using (this.cancelJoinWait)
							{
								this.cancelJoinWait.Cancel();
							}
						}
					}
					break;

					case "➕":
					{
						if (!this.Players.ContainsKey(reaction.User.Value.Id))
						{
							this.Players.TryAdd(reaction.User.Value.Id, new TriviaPlayer(reaction.User.Value.Username));
						}
					}
					break;
				}
			}
			else
			{
				if (this.Players.TryGetValue(reaction.User.Value.Id, out TriviaPlayer player))
				{
					if (this.answerDict.TryGetValue(reaction.Emote, out string answer) && !player.HasChosen)
					{
						if (answer == this.question.CorrectAnswer)
						{
							player.Correct++;
							player.Streak++;
							player.Score += 50 + 50 * player.Streak;
						}
						else
						{
							player.Wrong++;
							player.Streak = 0;
						}

						player.HasChosen = true;
						this.answered++;

						if (this.answered == this.Players.Count)
						{
							this.answered = 0;
							this.CancelWait();
						}
					}
				}
			}
		}

		public async Task StartTimeout()
		{
			foreach (TriviaQuestion triviaQuestion in this.trivia.Results)
			{
				foreach (TriviaPlayer player in this.Players.Values)
				{
					player.HasChosen = false;
				}

				this.answerDict.Clear();
				await this.message.RemoveAllReactionsAsync();
				this.question = triviaQuestion;

				if (triviaQuestion.IsBoolean)
				{
					await this.message.AddReactionsAsync(this.TrueFalseEmotes);
					this.answerDict.Add(this.TrueFalseEmotes[0], "True");
					this.answerDict.Add(this.TrueFalseEmotes[1], "False");
				}
				else
				{
					string[] shuffled = triviaQuestion.ShuffledItems;
					await this.message.AddReactionsAsync(this.MultiChoiceEmotes);

					for (int i = 0; i < this.MultiChoiceEmotes.Length; i++)
					{
						this.answerDict.Add(this.MultiChoiceEmotes[i], HttpUtility.HtmlDecode(shuffled[i]));
					}
				}

				EmbedBuilder builder = new EmbedBuilder()
				{
					Title = HttpUtility.HtmlDecode(triviaQuestion.Category),
					Description = HttpUtility.HtmlDecode(triviaQuestion.Question),
					Fields = this.answerDict.Select(x => new EmbedFieldBuilder().WithIsInline(true).WithName(x.Key.Name).WithValue(x.Value)).ToList(),
					Color = triviaQuestion.Difficulty switch
					{
						"easy"   => Color.Green,
						"medium" => Color.Gold,
						"hard"   => Color.Red,
						_        => Color.Default
					}
				};

				await this.message.ModifyAsync(m => m.Embed = builder.Build());

				try
				{
					await Task.Delay(30000, this.cancelQuestionWait.Token);
				}
				catch { /* Ignored */ }

				string answer = "Correct answer: " + HttpUtility.HtmlDecode(triviaQuestion.CorrectAnswer);

				foreach ((IEmote key, string value) in this.answerDict)
				{
					if (value == HttpUtility.HtmlDecode(triviaQuestion.CorrectAnswer))
					{
						answer += $" ({key.Name})";
						break;
					}
				}

				await this.message.ModifyAsync(m => m.Embed = new EmbedBuilder()
				{
					Title = "Answer Result",
					Color = Color.DarkGreen,
					Description = answer,
				}.Build());

				this.answered = 0;
				await Task.Delay(3000);
			}

			await this.message.RemoveAllReactionsAsync();
			await this.message.ModifyAsync(x => x.Embed = new EmbedBuilder()
			{
				Title = "Game Results!",
				Description = "Here are the top 10 winners!",
				Fields = this.Players.Take(10).OrderByDescending(y => y.Value.Score).Select(z => new EmbedFieldBuilder().WithIsInline(true).WithName(z.Value.Username).WithValue("Score: " + z.Value.Score)).ToList()
			}.Build());
		}
	}
}