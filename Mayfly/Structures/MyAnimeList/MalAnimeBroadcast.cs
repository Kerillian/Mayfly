using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeBroadcast
	{
		[JsonProperty("day")]
		public string Day { get; set; }

		[JsonProperty("time")]
		public string Time { get; set; }

		[JsonProperty("timezone")]
		public string Timezone { get; set; }

		[JsonProperty("string")]
		public string String { get; set; }
	}
}