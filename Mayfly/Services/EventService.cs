using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Logging;
using Mayfly.Extensions;
using Color = Discord.Color;

namespace Mayfly.Services
{
	public class EventService
	{
		private readonly DiscordSocketClient client;
		private readonly InteractionService interaction;
		private readonly DatabaseService database;
		private readonly ILogger logger;
		private readonly IServiceProvider provider;

		public EventService(DiscordSocketClient dsc, InteractionService @is, DatabaseService db, ILogger il, IServiceProvider isp)
		{
			client = dsc;
			interaction = @is;
			database = db;
			logger = il;
			provider = isp;
			
			client.InteractionCreated += HandleInteraction;
			interaction.ModalCommandExecuted += HandleModal;
			interaction.SlashCommandExecuted += HandleExecution;
			interaction.Log += OnLogAsync;

			client.Log += OnLogAsync;

			if (logger is EventLogger log)
			{
				log.LogMessage += OnLogAsync;
			}
		}

		private async Task HandleModal(ModalCommandInfo info, IInteractionContext context, IResult result)
		{
			if (!result.IsSuccess && result is MayflyResult mResult)
			{
				EmbedBuilder embed = new EmbedBuilder()
				{
					Title = "Error: " + mResult.ErrorReason,
					Color = Color.Red,
					Description = mResult.Message
				};

				if (mResult.Error == InteractionCommandError.ParseFailed)
				{
					embed.Color = Color.Orange;
				}

				await context.Interaction.RespondOrFollowup(embed: embed.Build(), ephemeral: true);
			}
		}

		private async Task HandleExecution(SlashCommandInfo info, IInteractionContext context, IResult result)
		{
			if (result.IsSuccess)
			{
				await database.ModifyUserAsync(context.User, data =>
				{
					data.Experience += 1;
					data.Invokes += 1;
				});
				
				return;
			}

			EmbedBuilder embed;
				
			if (result is MayflyResult mResult)
			{
				embed = new EmbedBuilder()
				{
					Title = "Error: " + mResult.ErrorReason,
					Color = Color.Red,
					Description = mResult.Message
				};

				if (mResult.Error == InteractionCommandError.ParseFailed)
				{
					embed.Color = Color.Orange;
				}

				await context.Interaction.RespondOrFollowup(embed: embed.Build(), ephemeral: true);
				return;
			}

			embed = new EmbedBuilder()
			{
				Title = "Error: " + (result.Error?.ToString() ?? "Oof"),
				Color = Color.Red,
				Description = result.ErrorReason ?? "Something broke."
			};

			await context.Interaction.RespondOrFollowup(embed: embed.Build(), ephemeral: true);
		}

		private async Task HandleInteraction(SocketInteraction si)
		{
			try
			{
				SocketInteractionContext ctx = new SocketInteractionContext(client, si);
				await interaction.ExecuteCommandAsync(ctx, provider);
			}
			catch (Exception ex)
			{
				Embed embed = new EmbedBuilder()
				{
					Title = "Error: " + ex.GetType(),
					Color = Color.Red,
					Description = ex.Message
				}.Build();

				await si.RespondOrFollowup(embed: embed, ephemeral: true);
			}
		}
		
		private static Task OnLogAsync(LogMessage msg)
		{
			string txt = $"{DateTime.Now,-8:hh:mm:ss} {$"[{msg.Severity}]",-9} {msg.Source,-8} | {msg.Exception?.ToString() ?? msg.Message}";
			return Console.Out.WriteLineAsync(txt);
		}

		private static void OnLogAsync(object? obj, LogMessageEventArgs args)
		{
			string txt = $"{DateTime.Now,-8:hh:mm:ss} {$"[{args.Level}]",-9} {args.Source,-8} | {args.Exception?.ToString() ?? args.Message}";
			Console.WriteLine(txt);
		}
	}
}