using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeTrailerImages
	{
		[JsonProperty("image_url")]
		public string ImageUrl { get; set; }

		[JsonProperty("small_image_url")]
		public string SmallImageUrl { get; set; }

		[JsonProperty("medium_image_url")]
		public string MediumImageUrl { get; set; }

		[JsonProperty("large_image_url")]
		public string LargeImageUrl { get; set; }

		[JsonProperty("maximum_image_url")]
		public string MaximumImageUrl { get; set; }
	}
}