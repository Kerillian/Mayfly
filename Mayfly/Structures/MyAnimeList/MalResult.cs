using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalResult
	{
		[JsonProperty("mal_id")]
		public int MyAnimeListID { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("image_url")]
		public string ImageUrl { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("airing")]
		public bool Airing { get; set; }

		[JsonProperty("synopsis")]
		public string Synopsis { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("episodes")]
		public int Episodes { get; set; }

		[JsonProperty("score")]
		public float Score { get; set; }

		[JsonProperty("start_date")]
		public string StartDate { get; set; }

		[JsonProperty("end_date")]
		public string EndDate { get; set; }

		[JsonProperty("members")]
		public int Members { get; set; }

		[JsonProperty("rated")]
		public string Rated { get; set; }
	}
}