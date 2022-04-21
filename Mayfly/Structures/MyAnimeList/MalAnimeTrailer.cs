using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeTrailer
	{
		[JsonProperty("youtube_id")]
		public string YouTubeId { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("embed_url")]
		public string EmbedUrl { get; set; }

		[JsonProperty("images")]
		public MalAnimeTrailerImages Images { get; set; }
	}
}