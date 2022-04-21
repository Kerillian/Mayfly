using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeAired
	{
		[JsonProperty("from")]
		public string From { get; set; }

		[JsonProperty("to")]
		public string To { get; set; }

		[JsonProperty("prop")]
		public MalAnimeAiredProp Prop { get; set; }
		
		[JsonProperty("string")]
		public string String { get; set; }
	}
}