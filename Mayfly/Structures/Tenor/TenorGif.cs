using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TenorGif
	{
		[JsonProperty("created")]
		public float Created { get; set; }
		
		[JsonProperty("hasaudio")]
		public bool HasAudio { get; set; }
		
		[JsonProperty("id")]
		public string Id { get; set; }
		
		[JsonProperty("media")]
		public TenorFormats[] Media { get; set; }
		
		[JsonProperty("tags")]
		public string[] Tags { get; set; }
		
		[JsonProperty("title")]
		public string Title { get; set; }
		
		[JsonProperty("itemurl")]
		public string ItemUrl { get; set; }
		
		[JsonProperty("hascaption")]
		public bool HasCaption { get; set; }
		
		[JsonProperty("url")]
		public string Url { get; set; }
	}
}