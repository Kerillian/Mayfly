using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalTimeProp
	{
		[JsonProperty("day")]
		public int? Day { get; set; }

		[JsonProperty("month")]
		public int? Month { get; set; }

		[JsonProperty("year")]
		public int? Year { get; set; }
	}
}