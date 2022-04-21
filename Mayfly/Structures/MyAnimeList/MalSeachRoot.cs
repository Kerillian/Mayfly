using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalSeachRoot
	{
		[JsonProperty("pagination")]
		public MalPagination Pagination { get; set; }

		[JsonProperty("data")]
		public MalAnime[] Data { get; set; }
	}
}