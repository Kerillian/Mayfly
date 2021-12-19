﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Discord;
using Lavalink4NET.Player;
using Mayfly.Modules;

namespace Mayfly.Extensions
{
	public static class LavaLinkExtension
	{
		public const string DEFAULT_THUMBNAIL = "https://i.imgur.com/jR8gyDi.png";
		
		private static async ValueTask<string> GetThumbnail(this LavalinkTrack track)
		{
			if (track.Source is null)
			{
				return DEFAULT_THUMBNAIL;
			}
			
			(bool search, string url) = track.Source?.ToLower() switch {
				var yt when yt.Contains("youtube")
					=> (false, $"https://img.youtube.com/vi/{track.TrackIdentifier}/maxresdefault.jpg"),

				var twitch when twitch.Contains("twitch")
					=> (true, $"https://api.twitch.tv/v4/oembed?url={track.Source}"),

				var sc when sc.Contains("soundcloud")
					=> (true, $"https://soundcloud.com/oembed?url={track.Source}&format=json"),

				var vim when vim.Contains("vimeo")
					=> (false, $"https://i.vimeocdn.com/video/{track.TrackIdentifier}.png"),

				_ => (false, DEFAULT_THUMBNAIL)
			};

			if (!search)
			{
				return url;
			}

			using HttpClient client = new HttpClient();
			HttpResponseMessage response = await client.GetAsync(url);
			using HttpContent content = response.Content;
			await using Stream stream = await content.ReadAsStreamAsync();
			JsonDocument document = await JsonDocument.ParseAsync(stream);
						
			return document.RootElement.TryGetProperty("thumbnail_url", out JsonElement thumbnail) ? $"{thumbnail}" : url;
		}
		
		public static async Task<Embed> GetEmbedAsync(this LavalinkTrack track)
		{
			RequestInfo info = track.Context as RequestInfo;
			string name = info?.User?.Username ?? "Mayfly";
			string url = info?.User?.GetAvatarUrl(ImageFormat.Auto, 64) ?? DEFAULT_THUMBNAIL;
			
			return new EmbedBuilder()
			{
				Title = track.Title,
				Url = track.Source,
				ThumbnailUrl = await GetThumbnail(track),
				Color = Color.DarkGreen,
				
				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder().WithName("Channel").WithValue(track.Author).WithIsInline(true),
					new EmbedFieldBuilder().WithName("Duration").WithValue(track.Duration).WithIsInline(true)
				},
				
				Footer = new EmbedFooterBuilder()
				{
					Text = $"Queued by {name}",
					IconUrl = url
				}
			}.Build();
		}

		public static async Task<Embed> GetEmbedAsync(this LavalinkTrack track, string prefix)
		{
			RequestInfo info = track.Context as RequestInfo;
			string name = info?.User?.Username ?? "Mayfly";
			string url = info?.User?.GetAvatarUrl(ImageFormat.Auto, 64) ?? DEFAULT_THUMBNAIL;
			
			return new EmbedBuilder()
			{
				Title = $"{prefix}: {track.Title}",
				Url = track.Source,
				ThumbnailUrl = await GetThumbnail(track),
				Color = Color.DarkGreen,

				Fields = new List<EmbedFieldBuilder>()
				{
					new EmbedFieldBuilder().WithName("Channel").WithValue(track.Author).WithIsInline(true),
					new EmbedFieldBuilder().WithName("Duration").WithValue(track.Duration).WithIsInline(true)
				},
				
				Footer = new EmbedFooterBuilder()
				{
					Text = $"Queued by {name}",
					IconUrl = url
				}
			}.Build();
		}

		public static ICollection<string> GetQueuePaged(this VoteLavalinkPlayer player, int items)
		{
			List<string> pages = new List<string>();
			List<string> lines = new List<string>();
			int count = 0;

			foreach (LavalinkTrack track in player.Queue)
			{
				string line = $"{++count}. {track.Title}";

				if (count % (items + 1) == 0)
				{
					lines.Reverse();
					pages.Add(string.Join("\n", lines));
					lines.Clear();
				}

				lines.Add(line);
			}

			lines.Reverse();
			pages.Add(string.Join("\n", lines));
			return pages;
		}
	}
}