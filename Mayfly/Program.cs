using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Mayfly.Database;
using Mayfly.Services;
using Mayfly.Services.Poll;
using Mayfly.Services.Trivia;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Logging;
using Lavalink4NET.MemoryCache;
using Mayfly.Modules;

namespace Mayfly
{
	public class Program
	{
		private readonly CancellationTokenSource cancelToken = new CancellationTokenSource();

		public static async Task Main()
		{
			await new Program().StartAsync();
		}

		private static ServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()
				{
					LogLevel = LogSeverity.Verbose,
					HandlerTimeout = 5000,
					MessageCacheSize = 1000,
					DefaultRetryMode = RetryMode.RetryRatelimit,
				}))
				.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
				.AddSingleton(JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText("Config.json")))
				
				.AddDbContext<MayflyContext>()
				.AddSingleton<DatabaseService>()
				
				.AddSingleton<RateLimitService>()
				.AddSingleton<EventService>()
				.AddSingleton<PaginationService>()
				.AddSingleton<RandomService>()
				.AddSingleton<RouletteService>()
				.AddSingleton<AkinatorService>()
				.AddSingleton<PollService>()
				.AddSingleton<HttpService>()
				.AddSingleton<SteamService>()
				.AddSingleton<TriviaService>()
				.AddSingleton<WaifulabsService>()
				
				.AddSingleton<IAudioService, LavalinkNode>()
				.AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
				.AddSingleton<ILogger, EventLogger>()
				.AddSingleton(x =>
				{
					BotConfig? cfg = x.GetService<BotConfig>();
					
					return new LavalinkNodeOptions()
					{
						RestUri = $"http://{cfg?.LavaLinkIP}:{cfg?.LavaLinkPort}",
						WebSocketUri = $"ws://{cfg?.LavaLinkIP}:{cfg?.LavaLinkPort}",
						Password = cfg?.LavaLinkPassword ?? "youshallnotpass"
					};
				})
				.AddSingleton<ILavalinkCache, LavalinkCache>()
				.BuildServiceProvider();
		}

		private void OnCancel(object? sender, ConsoleCancelEventArgs args)
		{
			args.Cancel = true;

			using (cancelToken)
			{
				cancelToken.Cancel();
				Thread.Sleep(1000);
			}
		}

		private async Task StartAsync()
		{
			Console.CancelKeyPress += OnCancel;

			ServiceProvider provider = ConfigureServices();
			DiscordSocketClient client = provider.GetRequiredService<DiscordSocketClient>();
			InteractionService interaction = provider.GetRequiredService<InteractionService>();
			BotConfig config = provider.GetRequiredService<BotConfig>();
			MayflyContext context = provider.GetRequiredService<MayflyContext>();
			IAudioService audio = provider.GetRequiredService<IAudioService>();

			provider.GetRequiredService<EventService>();

			await context.Database.EnsureCreatedAsync();
			await interaction.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
			await client.LoginAsync(TokenType.Bot, provider.GetRequiredService<BotConfig>().Token);

			await client.StartAsync();

			client.Ready += async () =>
			{
				#if DEBUG
					await interaction.RegisterCommandsToGuildAsync(config.DebugID);
				#else
					await interaction.RegisterCommandsGloballyAsync();
					await audio.InitializeAsync();
				#endif

				if (client.GetGuild(config.DebugID) is IGuild guild)
				{
					//await interaction.AddModulesToGuildAsync(guild, true, interaction.GetModuleInfo<RootModule>());
				}
			};

			try
			{
				await Task.Delay(-1, cancelToken.Token);
			}
			catch (TaskCanceledException)
			{
				await client.StopAsync();
				await client.LogoutAsync();
			}
		}
	}
}