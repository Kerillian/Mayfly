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
			http = hs;
			host = host;
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
			using (cancelDeleted)
			{
				cancelDeleted.Cancel();
				cancelQuestionWait.Cancel();
				cancelJoinWait.Cancel();
			}
		}

		public async Task Setup(SocketInteraction interaction, TriviaOptions options)
		{
			await interaction.RespondAsync(embed: presetEmbed, components: WaitingComponent);
			message = await interaction.GetOriginalResponseAsync();
			trivia = await http.GetJsonAsync<TriviaResult>(options.Build());
			Players.TryAdd(interaction.User.Id, new TriviaPlayer(interaction.User.Username));
			
			try
			{
				await Task.Delay(30000, cancelJoinWait.Token);
				cancelJoinWait = new CancellationTokenSource();
			}
			catch { /* Ignored */ }
		}

		public void CancelWait()
		{
			using (cancelQuestionWait)
			{
				cancelQuestionWait.Cancel();
			}

			cancelQuestionWait = new CancellationTokenSource();
		}
		
		public void HandleButtons(SocketMessageComponent interaction)
		{
			if (interaction.Message.Id != message.Id)
			{
				return;
			}
		
			if (waitingForPlayers)
			{
				switch (interaction.Data.CustomId)
				{
					case "start":
					{
						Console.WriteLine($"{interaction.User.Username}: Tried to start the game!");
						
						if (interaction.User.Id == host)
						{
							waitingForPlayers = false;
							
							Console.WriteLine($"{interaction.User.Username}: Started the game!");
							
							using (cancelJoinWait)
							{
								cancelJoinWait.Cancel();
							}
						}
					}
					break;
		
					case "join":
					{
						Console.WriteLine($"{interaction.User.Username}: Joined!");
						
						if (!Players.ContainsKey(interaction.User.Id))
						{
							Players.TryAdd(interaction.User.Id, new TriviaPlayer(interaction.User.Username));
						}
					}
					break;
				}
			}
			else
			{
				if (Players.TryGetValue(interaction.User.Id, out TriviaPlayer player))
				{
					if (answerDict.TryGetValue(OptionToEmote(interaction.Data.CustomId), out string answer) && !player.HasChosen)
					{
						Console.WriteLine($"{interaction.User.Username}: Chose -> {answer}");
						
						if (answer == question.CorrectAnswer)
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
						answered++;
		
						if (answered == Players.Count)
						{
							answered = 0;
							CancelWait();
						}
					}
				}
			}
		}

		public async Task StartTimeout()
		{
			foreach (TriviaQuestion triviaQuestion in trivia.Results)
			{
				if (cancelDeleted.IsCancellationRequested)
				{
					return;
				}
				
				foreach (TriviaPlayer player in Players.Values)
				{
					player.HasChosen = false;
				}

				answerDict.Clear();
				question = triviaQuestion;

				if (triviaQuestion.IsBoolean)
				{
					answerDict.Add(TRUE_EMOJI, "True");
					answerDict.Add(FALSE_EMOJI, "False");
				}
				else
				{
					string[] shuffled = triviaQuestion.ShuffledItems;

					answerDict.Add(A_EMOJI, shuffled[0]);
					answerDict.Add(B_EMOJI, shuffled[1]);
					answerDict.Add(C_EMOJI, shuffled[2]);
					answerDict.Add(D_EMOJI, shuffled[3]);
				}

				EmbedBuilder builder = new EmbedBuilder()
				{
					Title = HttpUtility.HtmlDecode(triviaQuestion.Category),
					Description = HttpUtility.HtmlDecode(triviaQuestion.Question),
					Fields = answerDict.Select(x => new EmbedFieldBuilder().WithIsInline(true).WithName(x.Key).WithValue(x.Value)).ToList(),
					Color = triviaQuestion.Difficulty switch
					{
						"easy"   => Color.Green,
						"medium" => Color.Gold,
						"hard"   => Color.Red,
						_        => Color.Default
					}
				};

				await message.ModifyAsync(m =>
				{
					m.Embed = builder.Build();
					m.Components = question.IsBoolean ? TrueFalseComponent : MultiChoiceComponent;
				});

				try
				{
					await Task.Delay(30000, cancelQuestionWait.Token);
				}
				catch { /* Ignored */ }

				string answer = "Correct answer: " + HttpUtility.HtmlDecode(triviaQuestion.CorrectAnswer);

				foreach ((string key, string value) in answerDict)
				{
					if (value == HttpUtility.HtmlDecode(triviaQuestion.CorrectAnswer))
					{
						answer += $" ({key})";
						break;
					}
				}

				await message.ModifyAsync(m => m.Embed = new EmbedBuilder()
				{
					Title = "Answer Result",
					Color = Color.DarkGreen,
					Description = answer,
				}.Build());

				answered = 0;
				await Task.Delay(3000);
			}

			TableBuilder table = new TableBuilder("Username", "Score", "Correct", "Wrong");

			foreach (TriviaPlayer player in Players.OrderByDescending(x => x.Value.Score).Take(10).Select(x => x.Value))
			{
				table.AddRow(player.Username, player.Score.ToString(), player.Correct.ToString(), player.Wrong.ToString());
			}

			await message.ModifyAsync(x =>
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