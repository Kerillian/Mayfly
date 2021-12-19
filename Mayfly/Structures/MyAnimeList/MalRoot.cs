using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalRoot
	{
		[JsonProperty("request_hash")]
		public string RequestHash { get; set; }

		[JsonProperty("request_cached")]
		public bool RequestCached { get; set; }

		[JsonProperty("request_cache_expiry")]
		public int RequestCacheExpiry { get; set; }

		[JsonProperty("results")]
		public MalResult[] Results { get; set; }

		[JsonProperty("last_page")]
		public int LastPage { get; set; }
	}
}