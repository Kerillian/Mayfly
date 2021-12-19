using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Logging;

namespace Mayfly.Services
{
	public class EventService
	{
		private readonly DiscordSocketClient client;
		private readonly CommandService commandService;
		private readonly HelpService help;
		private readonly BotConfig config;
		private readonly DatabaseService database;
		private readonly IAudioService audio;
		private readonly ILogger logger;
		private readonly IServiceProvider provider;

		public EventService(DiscordSocketClient dsc, CommandService cs, HelpService hs, BotConfig bc, DatabaseService db, IAudioService ias, ILogger il, IServiceProvider isp)
		{
			this.client = dsc;
			this.commandService = cs;
			this.help = hs;
			this.config = bc;
			this.database = db;
			this.audio = ias;
			this.logger = il;
			this.provider = isp;

			this.client.MessageReceived += HandleMessage;
			this.commandService.CommandExecuted += HandleExecution;

			this.client.Log += OnLogAsync;
			this.commandService.Log += OnLogAsync;

			if (logger is EventLogger log)
			{
				log.LogMessage += OnLogAsync;
			}
		}

		private async Task HandleExecution(Optional<CommandInfo> info, ICommandContext context, IResult result)
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

				if (result is MayflyResult mResult)
				{
					switch (mResult.Error)
					{
						case CommandError.Exception:
							await channel.SendMessageAsync(embed: new EmbedBuilder()
							{
								Title = "Error: " + mResult.Reason,
								Description = mResult.Message,
								Color = Color.Red
							}.Build());
							break;
						
						case CommandError.ParseFailed:
							await channel.SendMessageAsync(embed: new EmbedBuilder()
							{
								Title = "Parse Failed: " + mResult.Reason,
								Description = info.IsSpecified
									? mResult.Message + "\n" + Format.Code(help.GetCommandHelp(info.Value), "cs")
									: mResult.Message
							}.Build());
							break;
					}
					
					return;
				}

				switch (result.Error)
				{
					case CommandError.ParseFailed:
						if (info.IsSpecified)
						{
							await channel.SendMessageAsync(Format.Code(help.GetCommandHelp(info.Value), "cs"));
						}
						break;
					
					case CommandError.BadArgCount:
						if (info.IsSpecified)
						{
							await channel.SendMessageAsync(Format.Code(help.GetCommandHelp(info.Value), "cs"));
						}
						break;
				
					case CommandError.UnknownCommand:
						await context.Message.AddReactionAsync(new Emoji("❌"));
						break;
				
					default:
						string reason = result.ErrorReason ?? "Something broke.";
						
						/*
						 * Yes, the grammar bothered me this much.
						 */
						if (reason == "This command may only be invoked in an NSFW channel.")
						{
							reason = "This command may only be invoked in a NSFW channel.";
						}

						await channel.SendMessageAsync(embed: new EmbedBuilder()
						{
							Title = "Error: " + (result.Error?.ToString() ?? "Oof"),
							Description = reason,
							Color = Color.Red
						}.Build());
						
						break;
				}
			}
		}

		private async Task HandleMessage(SocketMessage message)
		{
			if (message is SocketUserMessage msg && !msg.Author.IsBot && message.Channel is SocketGuildChannel)
			{
				SocketCommandContext context = new SocketCommandContext(client, msg);
				int argPos = 0;

				if (msg.HasStringPrefix(config.Prefix, ref argPos))
				{
					await commandService.ExecuteAsync(context, argPos, provider);
				}
			}
		}
		
		private static Task OnLogAsync(LogMessage msg)
		{
			string txt = $"{DateTime.Now,-8:hh:mm:ss} {$"[{msg.Severity}]",-9} {msg.Source,-8} | {msg.Exception?.ToString() ?? msg.Message}";
			return Console.Out.WriteLineAsync(txt);
		}

		private static void OnLogAsync(object obj, LogMessageEventArgs args)
		{
			string txt = $"{DateTime.Now,-8:hh:mm:ss} {$"[{args.Level}]",-9} {args.Source,-8} | {args.Exception?.ToString() ?? args.Message}";
			Console.WriteLine(txt);
		}
	}
}