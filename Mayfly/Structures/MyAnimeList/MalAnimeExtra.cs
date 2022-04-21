using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeExtra
	{
		[JsonProperty("mal_id")]
		public int MalId { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }
	}
}