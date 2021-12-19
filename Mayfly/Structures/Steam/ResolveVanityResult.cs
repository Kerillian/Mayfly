using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class ResolveVanityResult
	{
		[JsonProperty("steamid")]
		public string SteamId { get; set; } 

		[JsonProperty("success")]
		public int Success { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonIgnore]
		public ulong? SteamId64 => ulong.TryParse(this.SteamId, out ulong id) ? id : (ulong?)null;
	}
}