using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class XkcdPost
	{
		[JsonProperty("year")]
		public int Year { get; set; }

		[JsonProperty("month")]
		public int Month { get; set; }

		[JsonProperty("day")]
		public int Day { get; set; }

		[JsonProperty("num")]
		public int Num { get; set; }

		[JsonProperty("link")]
		public string Link { get; set; }

		[JsonProperty("news")]
		public string News { get; set; }

		[JsonProperty("safe_title")]
		public string SafeTitle { get; set; }

		[JsonProperty("transcript")]
		public string Transcript { get; set; }

		[JsonProperty("alt")]
		public string Alt { get; set; }

		[JsonProperty("img")]
		public string Img { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }
	}
}