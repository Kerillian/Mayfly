using System.Text.RegularExpressions;
using SpotifyAPI.Web;

namespace Mayfly.Services
{
	public class SpotifyService
	{
		private static readonly Regex SpotifyUrl = new Regex(@"https?:\/\/(?:open\.spotify\.com)\/(?<type>\w+)\/(?<id>[\w-]{22})(?:\?si=(?:[\w-]{22}))?", RegexOptions.Compiled);
		private readonly SpotifyClient spotify;
		
		public SpotifyService(BotConfig config)
		{
			spotify = new SpotifyClient(SpotifyClientConfig
				.CreateDefault()
				.WithToken(new OAuthClient()
					.RequestToken(new ClientCredentialsRequest(config.SpotifyId, config.SpotifySecret))
					.Result.AccessToken));
		}
		
		public async Task<List<string>> Search(string url)
		{
			List<string> tracks = new List<string>();
			Match match = SpotifyUrl.Match(url);

			if (match.Success)
			{
				string type = match.Groups["type"].Value;
				string id = match.Groups["id"].Value;

				switch (type)
				{
					case "album":
					{
						await foreach (SimpleTrack track in spotify.Paginate((await spotify.Albums.Get(id)).Tracks))
						{
							tracks.Add($"{track.Name} {string.Join(' ', track.Artists.Select(x => x.Name))}");
						}
					} break;

					case "playlist":
					{
						await foreach (PlaylistTrack<IPlayableItem> item in spotify.Paginate((await spotify.Playlists.Get(id)).Tracks ?? throw new InvalidOperationException()))
						{
							if (item.Track is FullTrack track)
							{
								tracks.Add($"{track.Name} {string.Join(' ', track.Artists.Select(x => x.Name))}");
							}
						}
					} break;

					case "track":
					{
						FullTrack track = await spotify.Tracks.Get(id);
						tracks.Add($"{track.Name} {string.Join(" ", track.Artists.Select(x => x.Name))}");	
					} break;
				}
			}

			return tracks;
		}
	}
}