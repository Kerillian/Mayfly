using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalPaginationItems
	{
		[JsonProperty("count")]
		public int Count { get; set; }

		[JsonProperty("total")]
		public int Total { get; set; }

		[JsonProperty("per_page")]
		public int PerPage { get; set; }
	}
}