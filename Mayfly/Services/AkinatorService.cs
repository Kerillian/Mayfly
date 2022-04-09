using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Mayfly.Akinator;
using Mayfly.Akinator.Enumerations;
using Mayfly.Akinator.Model;

namespace Mayfly.Services
{
	public class ClientUserPair
	{
		public AkinatorClient Client { get; }
		public ulong UserId { get; }
		public IUserMessage Message { get; }
		public EmbedBuilder Embed { get; }
		public bool Requesting { get; set; }
		public DateTime LastUsed { get; set; }

		public ClientUserPair(AkinatorClient client, ulong id, IUserMessage message)
		{
			Client = client;
			UserId = id;
			Message = message;
			LastUsed = DateTime.Now.AddMinutes(2);

			Embed = new EmbedBuilder()
			{
				Color = new Color(255, 185, 12)
			};
		}

		public async Task UpdateMessage(ComponentBuilder components, bool stop = false)
		{
			await Message.ModifyAsync(msg =>
			{
				msg.Embed = Embed.Build();
				msg.Components = stop ? null : components.Build();
			});
		}
	}

	public class AkinatorService : IDisposable
	{
		private readonly Embed presetEmbed = new EmbedBuilder() { Description = "_Building message..._", Color = Color.Red }.Build();

		private readonly ComponentBuilder components = new ComponentBuilder()
			.WithButton("Yes", "yes")
			.WithButton("No", "no")
			.WithButton("Idk", "dontKnow")
			.WithButton("Probably", "probably", ButtonStyle.Secondary, row: 1)
			.WithButton("Probably not", "probablyNot", ButtonStyle.Secondary, row: 1)
			.WithButton("Undo", "undo", ButtonStyle.Danger, row: 2)
			.WithButton("Stop", "stop", ButtonStyle.Danger, row: 2);

		private readonly DiscordSocketClient discord;
		private ConcurrentDictionary<ulong, ClientUserPair> Clients;

		public AkinatorService(DiscordSocketClient dsc)
		{
			discord = dsc;
			Clients = new ConcurrentDictionary<ulong, ClientUserPair>();

			discord.ButtonExecuted += HandleButtons;
		}

		private async Task HandleButtons(SocketMessageComponent component)
		{
			if (Clients.TryGetValue(component.User.Id, out ClientUserPair pair))
			{
				await component.DeferAsync();
				
				if (!pair.Requesting && component.User.Id == pair.UserId)
				{
					pair.Requesting = true;
					await UpdateClient(pair, component.Data.CustomId);
				}
			}
		}

		public async Task UpdateClient(ClientUserPair pair, string action)
		{
			pair.LastUsed = DateTime.Now.AddMinutes(2);

			AkinatorQuestion question = null;

			try
			{
				switch (action)
				{
					case "yes":
						question = await pair.Client.Answer(AnswerOptions.Yes);
						break;

					case "no":
						question = await pair.Client.Answer(AnswerOptions.No);
						break;

					case "probably":
						question = await pair.Client.Answer(AnswerOptions.Probably);
						break;
					
					case "probablyNot":
						question = await pair.Client.Answer(AnswerOptions.ProbablyNot);
						break;
					
					case "dontKnow":
						question = await pair.Client.Answer(AnswerOptions.DontKnow);
						break;

					case "undo":
						if (pair.Client.CurrentQuestion.Step > 0)
						{
							question = await pair.Client.UndoAnswer();
						}
						break;

					case "stop":
						pair.Embed.Description = "Your lamp has been extinguished.";
						pair.Embed.Title = "";
						pair.Embed.Color = Color.Red;
						await Stop(pair);
						return;
				}
			}
			catch { /* ignore */ }

			if (question == null)
			{
				pair.Embed.Color = Color.Red;
				pair.Embed.Title = "Guac";
				pair.Embed.Description = "A man of pure enigmatic power.\n\n(_The Genie couldn't find your character, sorry :/)_";
				pair.Embed.ImageUrl = "https://i.imgur.com/ghcq4lo.png";
				await Stop(pair);
				return;
			}

			if (question.Progression > 94f)
			{
				AkinatorGuess[] guesses = await pair.Client.GetGuess();

				if (guesses.Length > 0)
				{
					AkinatorGuess guess = guesses[0];

					pair.Embed.Color = Color.Green;
					pair.Embed.Title = guess.Name;
					pair.Embed.Description = guess.Description;
					pair.Embed.ImageUrl = guess.PhotoPath.ToString();

					await pair.Message.RemoveAllReactionsAsync();
					await pair.UpdateMessage(components, true);
					pair.Client.Dispose();
					Clients.TryRemove(pair.UserId, out ClientUserPair _);
				}
				else
				{
					pair.Embed.Color = Color.Red;
					pair.Embed.Title = "Guac";
					pair.Embed.Description = "A man of pure enigmatic power.\n\n(_The Genie couldn't find your character, sorry :/)_";
					pair.Embed.ImageUrl = "https://i.imgur.com/ghcq4lo.png";
					await Stop(pair);
				}
			}
			else
			{
				pair.Embed.Title = "Step: " + (question.Step + 1) + " | Accuracy: " + question.Progression.ToString("0.00") + "%";
				pair.Embed.Description = question.Text;
				await pair.UpdateMessage(components);
				pair.Requesting = false;
			}
		}

		public async Task Cleanup()
		{
			foreach (ClientUserPair pair in Clients.Values)
			{
				if (DateTime.Now > pair.LastUsed)
				{
					pair.Embed.Description = "Your lamp has been extinguished.";
					pair.Embed.Title = "";
					pair.Embed.Color = Color.Red;
					await pair.UpdateMessage(components, true);
					pair.Client.Dispose();
					Clients.TryRemove(pair.UserId, out ClientUserPair _);
				}
			}
		}

		public async Task<bool> NewSessionAsync(SocketInteractionContext ctx, Language language, ServerType type)
		{
			await Cleanup();

			if (!Clients.ContainsKey(ctx.User.Id))
			{
				await ctx.Interaction.RespondAsync(embed: presetEmbed, components: components.Build());
				IUserMessage message = await ctx.Interaction.GetOriginalResponseAsync();
				AkinatorServerLocator serverLocator = new AkinatorServerLocator();
				IAkinatorServer server = await serverLocator.SearchAsync(language, type);

				ClientUserPair pair = new ClientUserPair(new AkinatorClient(server), ctx.User.Id, message);

				try
				{
					await pair.Client.StartNewGame();

					pair.Embed.Title = "Step: " + (pair.Client.CurrentQuestion.Step + 1) + " | Accuracy: " + pair.Client.CurrentQuestion.Progression.ToString("0.00") + "%";
					pair.Embed.Description = pair.Client.CurrentQuestion.Text;
					await pair.UpdateMessage(components);
				}
				catch
				{
					await Stop(pair);
					return false;
				}

				Clients.TryAdd(ctx.User.Id, pair);
				await pair.UpdateMessage(components);

				return true;
			}

			return false;
		}
		
		public async Task<bool> Stop(ClientUserPair pair)
		{
			await pair.UpdateMessage(components, true);
			pair.Client.Dispose();
			return Clients.TryRemove(pair.UserId, out ClientUserPair _);
		}

		public void Dispose()
		{
			foreach (ClientUserPair pair in Clients.Values)
			{
				pair.Client.Dispose();
			}

			Clients.Clear();
		}
	}
}