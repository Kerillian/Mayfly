using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Mayfly.Structures;

namespace Mayfly.Services
{
	public static class SteamEndPoints
	{
		public const string RESOLVE_VANITY_URL = "https://api.steampowered.com/ISteamUser/ResolveVanityURL/v1/?key={0}&format=json&vanityurl={1}";
		public const string GET_PLAYER_SUMMARIES_URL = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}";
		public const string AJAX_ALIASES_URL = "https://steamcommunity.com/profiles/{0}/ajaxaliases/";
	}
	
	public class SteamProfile
	{
		public static readonly Regex VanityUrl = new Regex(@"steamcommunity\.com\/id\/([a-zA-Z0-9.-]+)", RegexOptions.Compiled);
		public static readonly Regex ProfileUrl = new Regex(@"steamcommunity\.com\/profiles\/(\d+)", RegexOptions.Compiled);
		public static readonly Regex Steam2 = new Regex(@"STEAM_0:[0-1]:([0-9]{1,10})", RegexOptions.Compiled);
		public static readonly Regex Steam3 = new Regex(@"U:1:([0-9]{1,10})", RegexOptions.Compiled);
		public static readonly Regex Steam64 = new Regex(@"7656119([0-9]{10})", RegexOptions.Compiled);

		public bool Valid { get; }

		public string Username { get; private set; }
		public string Avatar { get; private set; }
		public string[] Aliases { get; private set; }

		public string SteamId { get; }
		public string SteamId3 { get; }
		public string SteamId64 { get; }

		public SteamProfile(ulong id)
		{
			ulong remainder = id - 76561197960265728L;
			ulong offset = remainder % 2L;

			this.Valid = true;
			this.SteamId = $"STEAM_0:{offset}:{(remainder - offset) / 2L}";
			this.SteamId3 = $"U:1:{id - 76561197960265728L}";
			this.SteamId64 = id.ToString();
		}

		public SteamProfile()
		{
			this.Valid = false;
		}

		public async Task Populate(HttpService http, BotConfig cfg)
		{
			SteamPlayerSummaries json = (await http.GetJsonAsync<SteamResponse<SteamPlayerSummaries>>(string.Format(SteamEndPoints.GET_PLAYER_SUMMARIES_URL, cfg.SteamKey, SteamId64))).Response;

			if (json.Players.Count > 0)
			{
				this.Username = json.Players[0].PersonaName;
				this.Avatar = json.Players[0].AvatarFull;
			}

			List<SteamAliases> aliases = await http.GetJsonAsync<List<SteamAliases>>(string.Format(SteamEndPoints.AJAX_ALIASES_URL, SteamId64));

			if (aliases.Count > 0)
			{
				this.Aliases = aliases.Select(a => a.NewName).ToArray();
			}
		}
	}

	public class SteamService
	{
		private readonly HttpService http;
		private readonly BotConfig config;
		
		public SteamService(HttpService hs, BotConfig bc)
		{
			this.http = hs;
			this.config = bc;
		}
		
		public async Task<SteamProfile> GetSteamID(string str)
		{
			Match url = SteamProfile.ProfileUrl.Match(str);

			if (url.Success)
			{
				if (ulong.TryParse(url.Groups[1].Value, out ulong id))
				{
					SteamProfile profile = new SteamProfile(id);
					await profile.Populate(this.http, this.config);
					return profile;
				}
			}

			Match vanity = SteamProfile.VanityUrl.Match(str);

			if (vanity.Success)
			{
				ResolveVanityResult json = (await this.http.GetJsonAsync<SteamResponse<ResolveVanityResult>>(string.Format(SteamEndPoints.RESOLVE_VANITY_URL, this.config.SteamKey, vanity.Groups[1].Value))).Response;

				if (json != null && json.Success == 1 && ulong.TryParse(json.SteamId, out ulong id))
				{
					SteamProfile profile = new SteamProfile(id);
					await profile.Populate(this.http, this.config);
					return profile;
				}
			}

			Match steam2 = SteamProfile.Steam2.Match(str);

			if (steam2.Success)
			{
				SteamProfile profile = new SteamProfile(76561197960265728L + ulong.Parse(str.Substring(10)) * 2L + ulong.Parse(str.Substring(8, 1)));
				await profile.Populate(this.http, this.config);
				return profile;
			}

			Match steam3 = SteamProfile.Steam3.Match(str);

			if (steam3.Success)
			{
				SteamProfile profile = new SteamProfile((ulong)(Convert.ToInt64(str.Substring(4)) + 76561197960265728L));
				await profile.Populate(this.http, this.config);
				return profile;
			}

			Match steam64 = SteamProfile.Steam64.Match(str);

			if (steam64.Success)
			{
				if (ulong.TryParse(str, out ulong id))
				{
					SteamProfile profile = new SteamProfile(id);
					await profile.Populate(this.http, this.config);
					return profile;
				}
			}

			return new SteamProfile();
		}
	}
}