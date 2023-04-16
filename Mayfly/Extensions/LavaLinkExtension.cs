using Discord;
using Lavalink4NET.Player;
using Lavalink4NET.Artwork;
using Mayfly.Utilities;
using Color = Discord.Color;

namespace Mayfly.Extensions
{
	public static class LavaLinkExtension
	{
		public const string DEFAULT_THUMBNAIL = "https://i.imgur.com/jR8gyDi.png";

		private static async ValueTask<string> GetThumbnail(this LavalinkTrack? track)
		{
			using ArtworkService artwork = new ArtworkService();
			
			if (track is { SourceName: { } })
			{
				Uri url = await artwork.ResolveAsync(track);
				return url is null ? DEFAULT_THUMBNAIL : url.ToString();
			}

			return DEFAULT_THUMBNAIL;
		}

		public static async Task<Embed> GetEmbedAsync(this LavalinkTrack track, string prefix = null, Color? color = null, string defaultThumbnail = null)
		{
			EmbedBuilder builder = new EmbedBuilder()
			{
				Title = prefix is null ? track?.Title : $"{prefix}: {track?.Title}",
				ThumbnailUrl = await track.GetThumbnail() ?? (defaultThumbnail ?? DEFAULT_THUMBNAIL),
				Color = color ?? Color.DarkGreen
			};

			builder.AddField("Channel", track.Author, true);
			builder.AddField("Duration", track.IsSeekable ? $"{(int)track.Duration.TotalMinutes}:{track.Duration.Seconds:00}" : "∞", true);

			if (track.Context is QueueInfo info)
			{
				builder.WithFooter($"Queued by: {info.User.Username}", info.User.GetAvatarUrl(size: 64));
			}

			return builder.Build();
		}

		public static IEnumerable<string> GetQueuePaged(this VoteLavalinkPlayer player, int items)
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