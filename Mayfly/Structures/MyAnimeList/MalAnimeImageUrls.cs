using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeImageUrls
	{
		[JsonProperty("image_url")]
		public string ImageUrl { get; set; }

		[JsonProperty("small_image_url")]
		public string SmallImageUrl { get; set; }

		[JsonProperty("large_image_url")]
		public string LargeImageUrl { get; set; }
	}
}