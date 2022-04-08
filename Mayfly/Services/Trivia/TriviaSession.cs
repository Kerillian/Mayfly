using System.Web;
using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Mayfly.Structures;
using Mayfly.Utilities;

namespace Mayfly.Services.Trivia
{
	public class TriviaSession
	{
		private const string A_EMOJI = "🇦";
		private const string B_EMOJI = "🇧";
		private const string C_EMOJI = "🇨";
		private const string D_EMOJI = "🇩";
		private const string TRUE_EMOJI = "✔️";
		private const string FALSE_EMOJI = "⭕";
		private const string JOIN_EMOJI = "➕";
		private const string START_EMOJI = "▶️";
		
		private readonly HttpService http;
		private ConcurrentDictionary<ulong, TriviaPlayer> Players = new ConcurrentDictionary<ulong, TriviaPlayer>();
		private IUserMessage message;
		private bool waitingForPlayers = true;
		private readonly ulong host;
		private CancellationTokenSource cancelQuestionWait = new CancellationTokenSource();
		private CancellationTokenSource cancelJoinWait = new CancellationTokenSource();
		private CancellationTokenSource cancelDeleted = new CancellationTokenSource();

		private readonly Embed presetEmbed = new EmbedBuilder()
		{
			Title = "Trivia Lobby",
			Description = "To play click the  icon below!\n\n_(If you are the host, you can click the ▶️ to start)_",
			Color = Color.Green,
		}.Build();

		private readonly MessageComponent WaitingComponent = new ComponentBuilder()
			.WithButton("Join", "join", ButtonStyle.Success, new Emoji(JOIN_EMOJI))
			.WithButton("Start", "start", ButtonStyle.Secondary, new Emoji(START_EMOJI))
			.Build();

		private readonly MessageComponent TrueFalseComponent = new ComponentBuilder()
			.WithButton("True", "true", ButtonStyle.Success, new Emoji(TRUE_EMOJI))
			.WithButton("False", "false", ButtonStyle.Danger, new Emoji(FALSE_EMOJI))
			.Build();

		private readonly MessageComponent MultiChoiceComponent = new ComponentBuilder()
			.WithButton(emote: new Emoji(A_EMOJI), customId: "a", style: ButtonStyle.Secondary)
			.WithButton(emote: new Emoji(B_EMOJI), customId: "b", style: ButtonStyle.Secondary)
			.WithButton(emote: new Emoji(C_EMOJI), customId: "c", style: ButtonStyle.Secondary)
			.WithButton(emote: new Emoji(D_EMOJI), customId: "d", style: ButtonStyle.Secondary)
			.Build();

		private TriviaResult trivia;
		private TriviaQuestion question;
		private readonly Dictionary<string, string> answerDict = new Dictionary<string, string>();
		private int answered;

		public TriviaSession(HttpService hs, ulong host)
		{
			this.http = hs;
			this.host = host;
		}

		private string OptionToEmote(string option)
		{
			return option switch
			{
				"a"     => A_EMOJI,
				"b"     => B_EMOJI,
				"c"     => C_EMOJI,
				"d"     => D_EMOJI,
				"true"  => TRUE_EMOJI,
				"false" => FALSE_EMOJI
			};
		}

		public bool IsMyMessage(ulong id)
		{
			return message.Id == id;
		}

		public void ForceStop()
		{
			using (this.cancelDeleted)
			{
				this.cancelDeleted.Cancel();
				this.cancelQuestionWait.Cancel();
				this.cancelJoinWait.Cancel();
			}
		}

		public async Task Setup(SocketInteraction interaction, TriviaOptions options)
		{
			await interaction.RespondAsync(embed: presetEmbed, components: WaitingComponent);
			this.message = await interaction.GetOriginalResponseAsync();
			this.trivia = await this.http.GetJsonAsync<TriviaResult>(options.Build());
			this.Players.TryAdd(interaction.User.Id, new TriviaPlayer(interaction.User.Username));
			
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
		
		public void HandleButtons(SocketMessageComponent interaction)
		{
			if (interaction.Message.Id != this.message.Id)
			{
				return;
			}
		
			if (this.waitingForPlayers)
			{
				switch (interaction.Data.CustomId)
				{
					case "start":
					{
						Console.WriteLine($"{interaction.User.Username}: Tried to start the game!");
						
						if (interaction.User.Id == this.host)
						{
							this.waitingForPlayers = false;
							
							Console.WriteLine($"{interaction.User.Username}: Started the game!");
							
							using (this.cancelJoinWait)
							{
								this.cancelJoinWait.Cancel();
							}
						}
					}
					break;
		
					case "join":
					{
						Console.WriteLine($"{interaction.User.Username}: Joined!");
						
						if (!this.Players.ContainsKey(interaction.User.Id))
						{
							this.Players.TryAdd(interaction.User.Id, new TriviaPlayer(interaction.User.Username));
						}
					}
					break;
				}
			}
			else
			{
				if (this.Players.TryGetValue(interaction.User.Id, out TriviaPlayer player))
				{
					if (this.answerDict.TryGetValue(OptionToEmote(interaction.Data.CustomId), out string answer) && !player.HasChosen)
					{
						Console.WriteLine($"{interaction.User.Username}: Chose -> {answer}");
						
						if (answer == this.question.CorrectAnswer)
						{
							player.Correct++;
							player.Streak++;
							player.Score += 50 + 50 * player.Streak;
						}
						else
						{
							if (player.Streak > player.BestStreak)
							{
								player.BestStreak = player.Streak;
							}
							
							player.Streak = 0;
							player.Wrong++;
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
				if (this.cancelDeleted.IsCancellationRequested)
				{
					return;
				}
				
				foreach (TriviaPlayer player in this.Players.Values)
				{
					player.HasChosen = false;
				}

				this.answerDict.Clear();
				this.question = triviaQuestion;

				if (triviaQuestion.IsBoolean)
				{
					this.answerDict.Add(TRUE_EMOJI, "True");
					this.answerDict.Add(FALSE_EMOJI, "False");
				}
				else
				{
					string[] shuffled = triviaQuestion.ShuffledItems;

					this.answerDict.Add(A_EMOJI, shuffled[0]);
					this.answerDict.Add(B_EMOJI, shuffled[1]);
					this.answerDict.Add(C_EMOJI, shuffled[2]);
					this.answerDict.Add(D_EMOJI, shuffled[3]);
				}

				EmbedBuilder builder = new EmbedBuilder()
				{
					Title = HttpUtility.HtmlDecode(triviaQuestion.Category),
					Description = HttpUtility.HtmlDecode(triviaQuestion.Question),
					Fields = this.answerDict.Select(x => new EmbedFieldBuilder().WithIsInline(true).WithName(x.Key).WithValue(x.Value)).ToList(),
					Color = triviaQuestion.Difficulty switch
					{
						"easy"   => Color.Green,
						"medium" => Color.Gold,
						"hard"   => Color.Red,
						_        => Color.Default
					}
				};

				await this.message.ModifyAsync(m =>
				{
					m.Embed = builder.Build();
					m.Components = this.question.IsBoolean ? TrueFalseComponent : MultiChoiceComponent;
				});

				try
				{
					await Task.Delay(30000, this.cancelQuestionWait.Token);
				}
				catch { /* Ignored */ }

				string answer = "Correct answer: " + HttpUtility.HtmlDecode(triviaQuestion.CorrectAnswer);

				foreach ((string key, string value) in this.answerDict)
				{
					if (value == HttpUtility.HtmlDecode(triviaQuestion.CorrectAnswer))
					{
						answer += $" ({key})";
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

			TableBuilder table = new TableBuilder("Username", "Score", "Correct", "Wrong");

			foreach (TriviaPlayer player in this.Players.OrderByDescending(x => x.Value.Score).Take(10).Select(x => x.Value))
			{
				table.AddRow(player.Username, player.Score.ToString(), player.Correct.ToString(), player.Wrong.ToString());
			}

			await this.message.ModifyAsync(x =>
			{
				x.Components = null;
				x.Embed = new EmbedBuilder()
				{
					Title = "Game Results!",
					Color = Color.Blue,
					Description = "Top 10 best scoring players!\n" + Format.Code(table.Build()),
				}.Build();
			});
		}
	}
}