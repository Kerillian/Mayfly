using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Logging;

namespace Mayfly.Services
{
	public class EventService
	{
		private readonly DiscordSocketClient client;
		private readonly InteractionService interaction;
		private readonly BotConfig config;
		private readonly DatabaseService database;
		private readonly IAudioService audio;
		private readonly ILogger logger;
		private readonly IServiceProvider provider;

		public EventService(DiscordSocketClient dsc, InteractionService @is, BotConfig bc, DatabaseService db, IAudioService ias, ILogger il, IServiceProvider isp)
		{
			this.client = dsc;
			this.interaction = @is;
			this.config = bc;
			this.database = db;
			this.audio = ias;
			this.logger = il;
			this.provider = isp;
			
			this.client.InteractionCreated += HandleInteraction;
			this.interaction.SlashCommandExecuted += HandleExecution;
			this.interaction.Log += OnLogAsync;

			this.client.Log += OnLogAsync;

			if (logger is EventLogger log)
			{
				log.LogMessage += OnLogAsync;
			}
		}

		private async Task HandleExecution(SlashCommandInfo info, IInteractionContext context, IResult result)
		{
			if (context.Channel is ISocketMessageChannel channel)
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

				Embed embed;
				if (result is MayflyResult { Error: InteractionCommandError.Exception or InteractionCommandError.Unsuccessful } mResult)
				{
					embed = new EmbedBuilder()
					{
						Title = "Error: " + mResult.ErrorReason,
						Color = Color.Red,
						Description = mResult.Message
					}.Build();

					if (context.Interaction.HasResponded)
					{
						await context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
					}
					else
					{
						await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
					}

					return;
				}

				embed = new EmbedBuilder()
				{
					Title = "Error: " + (result.Error?.ToString() ?? "Oof"),
					Color = Color.Red,
					Description = result.ErrorReason ?? "Something broke."
				}.Build();
						
				if (context.Interaction.HasResponded)
				{
					await context.Interaction.FollowupAsync(embed: embed, ephemeral: true);
				}
				else
				{
					await context.Interaction.RespondAsync(embed: embed, ephemeral: true);
				}
			}
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
				Console.WriteLine(ex);

				if(si.Type == InteractionType.ApplicationCommand)
				{
					await si.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
				}
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