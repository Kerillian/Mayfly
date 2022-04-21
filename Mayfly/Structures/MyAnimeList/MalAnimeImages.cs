using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnimeImages
	{
		[JsonProperty("jpg")]
		public MalAnimeImageUrls Jpg { get; set; }

		[JsonProperty("webp")]
		public MalAnimeImageUrls WebP { get; set; }
	}
}