using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class ImgurAdConfig
	{
		[JsonProperty("safeFlags")]
		public string[] SafeFlags { get; set; }

		[JsonProperty("unsafeFlags")]
		public object[] UnsafeFlags { get; set; }

		[JsonProperty("showsAds")]
		public bool ShowsAds { get; set; }
	}
}