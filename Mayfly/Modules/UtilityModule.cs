using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Mayfly.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json.Linq;
using Mayfly.Extensions;
using Mayfly.Services;
using Mayfly.Structures;
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
		
		[Command("ping"), Summary("Pong!")]
		public async Task Ping()
		{
			await ReplyAsync($"Pong! ({Context.Client.Latency}ms)");
		}

		[Command("about"), Summary("About me.")]
		public async Task About()
		{
			await ReplyAsync("", false, new EmbedBuilder()
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
		
		[GuildOwner, RequireBotPermission(GuildPermission.CreateInstantInvite), RequireContext(ContextType.Guild)]
		[Command("assistance"), Summary("Request bot assistance.")]
		public async Task Assistance()
		{
			if (Context.Channel is ITextChannel channel)
			{
				IInviteMetadata invite = await channel.CreateInviteAsync(null, 1, true, false);
				RestApplication rest = await Context.Client.GetApplicationInfoAsync();

				if (invite != null && rest?.Owner != null)
				{
					await rest.Owner.SendMessageAsync("", false, new EmbedBuilder()
					{
						Title = $"Assistance requested - {Context.Guild.Name}",
						Color = new Discord.Color(0x00C51C),
						Fields = new List<EmbedFieldBuilder>()
						{
							new EmbedFieldBuilder() { Name = "Owner", Value = $"{Context.User.Username}#{Context.User.Discriminator}", IsInline = true },
							new EmbedFieldBuilder() { Name = "Invite", Value = invite.Url, IsInline = true}
						}
					}.WithEmpty().Build());

					await ReplyAsync("I have sent an invite to the developer. Please note that he might be offline or afk.");
				}
			}
		}

		[Command("status"), Summary("Check discord server status.")]
		public async Task Status()
		{
			SPResult json = await Http.GetJsonAsync<SPResult>("https://srhpyqt94yxb.statuspage.io/api/v2/summary.json");

			EmbedBuilder builder = new EmbedBuilder()
			{
				Title = json.Status.Description,
				Color = new Discord.Color(0x738BD7),

				Fields = json.Components.Select(x => new EmbedFieldBuilder().WithIsInline(true).WithName(x.Name).WithValue(x.Status)).ToList()
			}.WithEmpty();

			await ReplyAsync("", false, builder.Build());
		}

		[Command("translate"), Summary("Google Translate in discord.")]
		public async Task Translate(string from, string to, [Remainder] string text)
		{
			from = Uri.EscapeDataString(from);
			to = Uri.EscapeDataString(to);
			text = Uri.EscapeDataString(text);

			JArray json = await Http.GetJArrayAsync($"https://translate.google.com/translate_a/single?client=gtx&sl={from}&tl={to}&dt=t&q={text}");

			if (json.HasValues && json[0].HasValues && json[0][0].HasValues)
			{
				await ReplyAsync(json[0][0][0].Value<string>());
			}
		}

		[Command("vmi"), Summary("Youtube 'View more info' for steam.")]
		public async Task VMI(string Url, uint StartTime = 0)
		{
			if (TryParseVideoID(Url, out string videoId))
			{
				if (StartTime > 0)
				{
					await ReplyCodeAsync(string.Format(VC1, videoId, videoId));
				}
				else
				{
					await ReplyCodeAsync(string.Format(VC2, videoId, videoId, StartTime));
				}
			}
		}

		[Command("age"), Summary("Get the servers birth date."), RequireContext(ContextType.Guild)]
		public async Task Age()
		{
			await ReplyAsync(Context.Guild.CreatedAt.Date.ToLongDateString());
		}

		[Command("joined"), Summary("Get the date the user joined.")]
		public async Task Joined()
		{
			if (Context.User is IGuildUser user && user.JoinedAt.HasValue)
			{
				await ReplyAsync(user.JoinedAt.Value.Date.ToLongDateString());
			}
		}

		[Command("joined"), Summary("Get the date the user joined.")]
		public async Task Joined(IGuildUser user)
		{
			if (user.JoinedAt.HasValue)
			{
				await ReplyAsync(user.JoinedAt.Value.Date.ToLongDateString());
			}
		}

		[Command("avatar"), Summary("Get users avatar.")]
		public async Task Avatar(IGuildUser user)
		{
			string url = user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl();
			await using Stream stream = await this.Http.GetStreamAsync(url);
			string ext = this.ExtMatcher.Match(url).Groups[1].Value;
			
			await this.ReplyFileAsync(stream, $"avatar{ext}");
		}

		[Command("color"), Summary("Preview colors in different formats.")]
		public async Task ColorPreview(Discord.Color color)
		{
			using Image<Rgba32> img = new Image<Rgba32>(256, 256, new Rgba32(color.R, color.G, color.B));
			EmbedBuilder builder = new EmbedBuilder()
			{
				Title = StringUtility.GetProperName(ColorUtility.FindClosestKnownColor(color)),
				ThumbnailUrl = "attachment://color.png",
				Color = new Discord.Color(color.R, color.G, color.B),

				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder()
					{
						Name = "RGB",
						Value = $"{color.R}, {color.G}, {color.B}",
						IsInline = true
					},
					new EmbedFieldBuilder()
					{
						Name = "HSV",
						Value = $"{color.GetHue():0.00}, {color.GetSaturation() * 100:0.00}, {color.GetBrightness() * 100:0.00}",
						IsInline = true
					},
					new EmbedFieldBuilder()
					{
						Name = "Hex",
						Value = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"),
						IsInline = true
					}
				}
			}.WithEmpty();

			await using MemoryStream stream = new MemoryStream();
			
			await img.SaveAsPngAsync(stream);
			stream.Seek(0, SeekOrigin.Begin);
			await this.Context.Channel.SendFileAsync(stream, "color.png", embed: builder.Build());
		}

		[Command("screenshot"), Summary("Screenshot a website.")]
		public async Task<RuntimeResult> Screenshot(string url)
		{
			if (!url.StartsWith("http"))
			{
				return MayflyResult.FromUserError("InvalidUrl", "Invalid url provided.");
			}
			
			await this.ReplyAsync("", false, new EmbedBuilder().WithImageUrl($"https://api.microlink.io/?url={Uri.EscapeDataString(url)}&screenshot=true&meta=false&embed=screenshot.url").Build());
			return MayflyResult.FromSuccess();
		}
		
		// [Command("compile"), Alias("run"), Summary("Compile and run code.")]
		// public async Task Compile([Remainder] string source)
		// {
		//	 if (this.TryGetCode(source, out string lang, out string code))
		//	 {
		//		 RexLanguage rlang = RexTesterService.GetLanguageFromString(lang);
		//		 string args = RexTesterService.GetDefaultCompileArgs(rlang);
		//
		//		 if (rlang != RexLanguage.None)
		//		 {
		//			 RexTesterJson json = await rex.Compile(rlang, code, "", args);
		//			 EmbedBuilder builder = new EmbedBuilder().WithTitle($"{Enum.GetName(rlang)} compile result").WithFooter(json.Stats);
		//
		//			 if (!string.IsNullOrEmpty(json.Errors))
		//			 {
		//				 builder.Color = Color.Red;
		//				 builder.Fields.Add(new EmbedFieldBuilder().WithName("Errors").WithValue(Format.Code(json.Errors)));
		//			 }
		//			 
		//			 if (!string.IsNullOrEmpty(json.Warnings))
		//			 {
		//				 builder.Color = Color.Gold;
		//				 builder.Fields.Add(new EmbedFieldBuilder().WithName("Warnings").WithValue(Format.Code(json.Warnings)));
		//			 }
		//
		//			 if (!string.IsNullOrEmpty(json.Result))
		//			 {
		//				 builder.Color = Color.Green;
		//				 
		//				 string result = json.Result.LazySubstring(0, EmbedBuilder.MaxEmbedLength - (builder.Length + 15));
		//				 builder.Fields.Add(new EmbedFieldBuilder().WithName("Result").WithValue(Format.Code(result)));
		//			 }
		//			 
		//			 await this.ReplyAsync("", false, builder.Build());
		//		 }
		//	 }
		// }

		// [Command("compile"), Alias("run"), Summary("Compile and run code.")]
		// public async Task Compile()
		// {
		//	 IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 10).FlattenAsync();
		//
		//	 foreach (IMessage message in messages)
		//	 {
		//		 if (this.TryGetCode(message.Content, out string _, out string _))
		//		 {
		//			 await this.Compile(message.Content);
		//			 break;
		//		 }
		//	 }
		// }
	}
}