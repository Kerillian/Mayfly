using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TenorFormats
	{
		[JsonProperty("gif")]
		public TenorMedia Gif { get; set; }
		
		[JsonProperty("mediumgif")]
		public TenorMedia MediumGif { get; set; }
		
		[JsonProperty("tinygif")]
		public TenorMedia TinyGif { get; set; }
		
		[JsonProperty("nanogif")]
		public TenorMedia NanoGif { get; set; }
		
		[JsonProperty("mp4")]
		public TenorMedia Mp4 { get; set; }
		
		[JsonProperty("loopedmp4")]
		public TenorMedia LoopedMp4 { get; set; }
		
		[JsonProperty("tinymp4")]
		public TenorMedia TinyMp4 { get; set; }
		
		[JsonProperty("nanomp4")]
		public TenorMedia NanoMp4 { get; set; }
		
		[JsonProperty("webm")]
		public TenorMedia Webm { get; set; }
		
		[JsonProperty("tinywebm")]
		public TenorMedia TinyWebm { get; set; }
		
		[JsonProperty("nanowebm")]
		public TenorMedia NanoWebm { get; set; }
	}
}