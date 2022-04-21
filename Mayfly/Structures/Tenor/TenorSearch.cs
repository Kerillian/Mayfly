using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TenorSearch
	{
		[JsonProperty("results")]
		public TenorGif[] Results { get; set; }

		[JsonProperty("next")]
		public int Next { get; set; }

		[JsonIgnore]
		public bool HasNext => Next > 0;
	}
}