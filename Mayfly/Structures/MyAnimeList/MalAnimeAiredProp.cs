using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeAiredProp
	{
		[JsonProperty("from")]
		public MalTimeProp From { get; set; }

		[JsonProperty("to")]
		public MalTimeProp To { get; set; }
	}
}