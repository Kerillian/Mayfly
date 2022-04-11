using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json.Linq;
using Mayfly.Extensions;
using Mayfly.Services;
using Mayfly.Utilities;

namespace Mayfly.Modules
{
	public class UtilityModule : MayflyModule
	{
		private const string VC1 = "[url=https://www.youtube.com/embed/{0}?autoplay=1&controls=0&disablekb=1&loop=1&playlist={1}&showinfo=0&iv_load_policy=3]View more info[/url]";
		private const string VC2 = "[url=https://www.youtube.com/embed/{0}?autoplay=1&controls=0&disablekb=1&loop=1&playlist={1}&showinfo=0&iv_load_policy=3&start={2}]View more info[/url]";
		private readonly Regex RegularMatcher = new Regex(@"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)", RegexOptions.Compiled);
		private readonly Regex ShortMatcher = new Regex(@"youtu\.be/(.*?)(?:\?|&|/|$)", RegexOptions.Compiled);
		private readonly Regex ExtMatcher = new Regex(@".+\/{2}.+\/{1}.+(\.\w+)\?*.*", RegexOptions.Compiled);

		public HttpService Http { get; set; }

		public bool TryParseVideoID(string url, out string id)
		{
			id = default(string);

			if (string.IsNullOrWhiteSpace(url))
				return false;

			string regularMatch = RegularMatcher.Match(url).Groups[1].Value;
			if (!string.IsNullOrWhiteSpace(regularMatch))
			{
				id = regularMatch;
				return true;
			}

			string shortMatch = ShortMatcher.Match(url).Groups[1].Value;
			if (string.IsNullOrWhiteSpace(shortMatch))
			{
				id = shortMatch;
				return true;
			}

			return false;
		}
		
		[SlashCommand("ping", "Pong!")]
		public async Task Ping()
		{
			await RespondAsync($"Pong! ({Context.Client.Latency}ms)");
		}

		[SlashCommand("about", "About me.")]
		public async Task About()
		{
			await RespondAsync(embed: new EmbedBuilder()
			{
				Title = Context.Client.CurrentUser.Username,
				Color = new Discord.Color(0xB4DC7A),
				ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),

				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder() { Name = "Developer", Value = "Kerillian", IsInline = true },
					new EmbedFieldBuilder() { Name = "Running", Value = $"Discord.Net v{DiscordConfig.Version}", IsInline = true }
				}
			}.Build());
		}

		[SlashCommand("translate", "Google Translate in discord.")]
		public async Task Translate(string from, string to, string text)
		{
			await DeferAsync();
			from = Uri.EscapeDataString(from);
			to = Uri.EscapeDataString(to);
			text = Uri.EscapeDataString(text);

			JArray json = await Http.GetJArrayAsync($"https://translate.google.com/translate_a/single?client=gtx&sl={from}&tl={to}&dt=t&q={text}");

			if (json.HasValues && json[0].HasValues && json[0][0].HasValues)
			{
				await FollowupAsync(json[0][0][0].Value<string>());
			}
		}

		[SlashCommand("vmi", "Youtube 'View more info' for steam.")]
		public async Task<RuntimeResult> VMI(string url, uint startTime = 0)
		{
			if (TryParseVideoID(url, out string videoId))
			{
				if (startTime > 0)
				{
					await RespondWithCodeAsync(string.Format(VC1, videoId, videoId));
				}
				else
				{
					await RespondWithCodeAsync(string.Format(VC2, videoId, videoId, startTime));
				}
				
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromUserError("InvalidInput", "Please provide a YouTube video.");
		}

		[SlashCommand("age", "Get the servers birth date."), RequireContext(ContextType.Guild)]
		public async Task Age()
		{
			await RespondAsync(Context.Guild.CreatedAt.Date.ToLongDateString());
		}

		[SlashCommand("joined", "Get the date the user joined.")]
		public async Task Joined(IGuildUser user = null)
		{
			user ??= Context.User as IGuildUser;
			if (user.JoinedAt.HasValue)
			{
				await RespondAsync(user.JoinedAt.Value.Date.ToLongDateString());
			}
		}

		[SlashCommand("avatar", "Get users avatar.")]
		public async Task Avatar(IGuildUser user)
		{
			string url = user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl();
			await using Stream stream = await Http.GetStreamAsync(url);
			string ext = ExtMatcher.Match(url).Groups[1].Value;
			
			await RespondWithFileAsync(stream, $"avatar{ext}");
		}
		
		[SlashCommand("color", "Preview colors in different formats.")]
		public async Task ColorPreview([MinValue(0), MaxValue(255)] int r, [MinValue(0), MaxValue(255)]int g, [MinValue(0), MaxValue(255)]int b)
		{
			Discord.Color col = new Discord.Color(r, g, b);
			using Image<Rgba32> img = new Image<Rgba32>(256, 256, new Rgba32((byte)r, (byte)g, (byte)b, 255));
			EmbedBuilder builder = new EmbedBuilder()
			{
				Title = StringUtility.GetProperName(ColorUtility.FindClosestKnownColor(col)),
				ThumbnailUrl = "attachment://color.png",
				Color = col,

				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder()
					{
						Name = "RGB",
						Value = $"{r}, {g}, {b}",
						IsInline = true
					},
					new EmbedFieldBuilder()
					{
						Name = "HSV",
						Value = $"{col.GetHue():0.00}, {col.GetSaturation() * 100:0.00}, {col.GetBrightness() * 100:0.00}",
						IsInline = true
					},
					new EmbedFieldBuilder()
					{
						Name = "Hex",
						Value = "#" + col.R.ToString("X2") + col.G.ToString("X2") + col.B.ToString("X2"),
						IsInline = true
					}
				}
			}.WithEmpty();

			await using MemoryStream stream = new MemoryStream();
			
			await img.SaveAsPngAsync(stream);
			stream.Seek(0, SeekOrigin.Begin);
			await RespondWithFileAsync(stream, "color.png", embed: builder.Build());
		}

		[SlashCommand("bobux", "Bobux to USD.")]
		public async Task Bobux(double bobux)
		{
			await RespondAsync($"That would be about `{bobux * 0.0125:C}`");
		}
	}
}