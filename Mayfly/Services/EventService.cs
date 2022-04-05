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


					if (context.Interaction.HasResponded)
					{
						await context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true);
					}
					else
					{
						await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
					}

					return;
				}

				embed = new EmbedBuilder()
				{
					Title = "Error: " + (result.Error?.ToString() ?? "Oof"),
					Color = Color.Red,
					Description = result.ErrorReason ?? "Something broke."
				};
						
				if (context.Interaction.HasResponded)
				{
					await context.Interaction.FollowupAsync(embed: embed.Build(), ephemeral: true);
				}
				else
				{
					await context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
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
				Embed embed = new EmbedBuilder()
				{
					Title = "Error: " + ex.GetType(),
					Color = Color.Red,
					Description = ex.Message
				}.Build();
						
				if (si.HasResponded)
				{
					await si.FollowupAsync(embed: embed, ephemeral: true);
				}
				else
				{
					await si.RespondAsync(embed: embed, ephemeral: true);
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