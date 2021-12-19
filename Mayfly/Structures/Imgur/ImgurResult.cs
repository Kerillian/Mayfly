using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class ImgurResult
	{
		[JsonProperty("data")]
		public ImgurPost[] Data { get; set; }

		[JsonProperty("success")]
		public bool Success { get; set; }

		[JsonProperty("status")]
		public int Status { get; set; }
	}
}