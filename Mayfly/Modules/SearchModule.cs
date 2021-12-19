using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Services;
using Mayfly.Structures;
using Mayfly.Utilities;

namespace Mayfly.Modules
{
	public class SearchModule : MayflyModule
	{
		public HttpService Http { get; set; }
		public PaginationService Pagination { get; set; }
		public SteamService Steam { get; set; }

		private static readonly Regex ReferenceBlockPattern = new Regex(@"\[([^\[\]]+)\]", RegexOptions.Multiline | RegexOptions.Compiled);

		[Command("define"), Summary("Defines a word using Urban Dictionary.")]
		public async Task Define([Remainder] string word)
		{
			UrbanResult json = await Http.GetJsonAsync<UrbanResult>($"http://api.urbandictionary.com/v0/define?term={Uri.EscapeDataString(word)}");

			if (json != null)
			{
				string definition = json.List[0].Definition;

				foreach (Match match in ReferenceBlockPattern.Matches(definition))
				{
					string str = match.Groups?[1].Value;

					if (!string.IsNullOrEmpty(str))
					{
						definition = definition.Replace($"[{str}]", $"[{str}](https://www.urbandictionary.com/define.php?term={Uri.EscapeDataString(str)})");
					}
				}

				if (definition.Length > 2048)
				{
					string RM = $"[... More]({json.List[0].Permalink})";
					definition = definition.Substring(0, 2048 - RM.Length) + RM;
				}

				await ReplyAsync("", false, new EmbedBuilder()
				{
					Title = json.List[0].Word,
					Color = new Color(0xD23725),
					Description = definition,
					Url = json.List[0].Permalink,
					ThumbnailUrl = "https://i.imgur.com/970G1hw.png",
					Footer = new EmbedFooterBuilder()
					{
						Text = json.List[0].Author,
						IconUrl = "https://i.imgur.com/wg2ISw5.png"
					}
				}.Build());
			}
		}

		[Command("mal"), Alias("anime"), Summary("Search my anime list.")]
		public async Task MyAnimeList([Remainder] string query)
		{
			MalRoot data = await Http.GetJsonAsync<MalRoot>($"https://api.jikan.moe/v3/search/anime?q={Uri.EscapeDataString(query)}");

			if (data != null)
			{
				IEnumerable<EmbedBuilder> builders = data.Results.Select(x => new EmbedBuilder().WithTitle(x.Title).WithDescription(x.Synopsis).WithThumbnailUrl(x.ImageUrl).WithUrl(x.Url));

				await Pagination.SendMessageAsync(Context.Channel, new PaginatedMessage(builders, null, Color.Green, Context.User, new AppearanceOptions()
				{
					Timeout = TimeSpan.FromMinutes(3),
					Style = DisplayStyle.Selector
				}));
			}
		}

		[Command("trace"), Summary("Trace.moe in discord.")]
		public async Task<RuntimeResult> TraceMoe(string url)
		{
			TraceMoeResult json = await Http.GetJsonAsync<TraceMoeResult>("https://trace.moe/api/search?url=" + Uri.EscapeDataString(url));
			
			if (json is {Docs.Count: > 0})
			{
				IEnumerable<EmbedBuilder> builders = json.Docs.Select(x => new EmbedBuilder().WithTitle(x.Title).WithDescription(x.TitleEnglish).WithThumbnailUrl(x.ImageThumbnail));
				
				await Pagination.SendMessageAsync(Context.Channel, new PaginatedMessage(builders, null, Color.Green, Context.User, new AppearanceOptions()
				{
					Timeout = TimeSpan.FromMinutes(3),
					Style = DisplayStyle.Selector
				}));
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("NothingFound", "Couldn't find anything for that, sorry.");
		}

		[Command("trace"), Summary("Trace.moe in discord.")]
		public async Task<RuntimeResult> TraceMoe()
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.TraceMoe(url);
			}

			return MayflyResult.FromError("InvalidAttachment", "Attachment provided by user is invalid.");
		}

		[Command("mc"), Summary("Get minecraft profile info.")]
		public async Task<RuntimeResult> MCName(string username)
		{
			MinecraftProfile profile = await Http.GetJsonAsync<MinecraftProfile>($"https://api.mojang.com/users/profiles/minecraft/{Uri.EscapeDataString(username)}");

			if (profile is not null)
			{
				List<MinecraftNameHistory> names = await Http.GetJsonAsync<List<MinecraftNameHistory>>($"https://api.mojang.com/user/profiles/{profile.ID}/names");

				EmbedBuilder builder = new EmbedBuilder()
				{
					Title = profile.Name,
					Description = profile.ID,
					Color = Color.Green,
					ThumbnailUrl = $"https://crafatar.com/avatars/{profile.ID}.png?overlay=true"
				};

				if (names is { Count: > 0 })
				{
					foreach (MinecraftNameHistory name in names.Reverse<MinecraftNameHistory>())
					{
						string date = name.ChangedToAt.HasValue ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(name.ChangedToAt.Value).ToString("MM/dd/yyyy") : "No date.";
						builder.AddField(name.Name, date, true);
					}
				}

				await ReplyAsync("", false, builder.Build());
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("InvalidUsername", "Provided username was not found.");
		}

		[Command("steamid"), Summary("SteamID resolver.")]
		public async Task<RuntimeResult> SteamIDResolver(string id)
		{
			SteamProfile profile = await Steam.GetSteamID(id);

			if (profile.Valid)
			{
				StringBuilder builder = new StringBuilder();

				builder.AppendLine("SteamID   : " + profile.SteamId);
				builder.AppendLine("SteamID3  : " + profile.SteamId3);
				builder.AppendLine("SteamID64 : " + profile.SteamId64);
				builder.AppendLine("Aliases   : " + (profile.Aliases != null ? string.Join(", ", profile.Aliases) : "None"));
				
				await this.ReplyAsync("", false, new EmbedBuilder()
				{
					Title = profile.Username,
					Description = Format.Code(builder.ToString()),
					ThumbnailUrl = profile.Avatar,
					Url = $"https://steamcommunity.com/profiles/{profile.SteamId64}",
					Color = new Color(0x1B2838)
				}.Build());
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("ResolverFailure", "Failed to resolve SteamID.");
		}

		[Command("pwned"), Summary("https://haveibeenpwned.com/Passwords")]
		public async Task Pwned([Remainder] string password)
		{
			string hash = HashUtility.Sha1(password);
			string start = hash[..5];
			int times = 0;

			await using Stream stream = await this.Http.GetStreamAsync("https://api.pwnedpasswords.com/range/" + start);
			using StreamReader reader = new StreamReader(stream);
			
			while (!reader.EndOfStream)
			{
				string line = await reader.ReadLineAsync();
				string lineHash = string.Concat(start, line.AsSpan(0, 35));

				if (lineHash == hash)
				{
					times = int.Parse(line[36..]);
					break;
				}
			}

			await this.ReplyAsync($"Found that password `{times:#,0}` {(times == 1 ? "time." : "times.")}");
		}

		[Command("baro"), Summary("Is Baro Ki'Teer here?")]
		public async Task Baro()
		{
			VoidTraderInfo json = await Http.GetJsonAsync<VoidTraderInfo>("https://api.warframestat.us/pc/voidTrader");

			if (json is not null)
			{
				if (json.StartString.StartsWith('-'))
				{
					StringBuilder builder = new StringBuilder();
				
					foreach (VoidTraderItem item in json.Inventory)
					{
						builder.AppendLine($"# {item.Item}");
						builder.AppendLine($"{item.Ducats:#,0}d, {item.Credits:#,0}c");
						builder.AppendLine();
					}

					await ReplyAsync("", false, new EmbedBuilder()
					{
						Title = "Baro Ki'Teer's inventory",
						Color = new Color(0xFFC83D),
						Description = Format.Code(builder.ToString(), "markdown")
					}.Build());
				}
				else
				{
					await ReplyAsync($"Baro Ki'Teer will arrive in {json.StartString}");
				}
			}
			else
			{
				await ReplyAsync("Baro Ki'Teer is gone, dead, not alive.");
			}
		}
	}
}