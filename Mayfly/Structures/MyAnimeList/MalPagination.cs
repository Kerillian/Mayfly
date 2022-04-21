using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalPagination
	{
		[JsonProperty("last_visible_page")]
		public int LastVisiblePage { get; set; }

		[JsonProperty("has_next_page")]
		public bool HasNextPage { get; set; }

		[JsonProperty("current_page")]
		public int CurrentPage { get; set; }

		[JsonProperty("items")]
		public MalPaginationItems Items { get; set; }
	}
}