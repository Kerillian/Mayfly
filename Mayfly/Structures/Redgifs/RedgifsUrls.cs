using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class RedgifsUrls
	{
		[JsonProperty("sd")]
		public string SD { get; set; }
		
		[JsonProperty("hd")]
		public string HD { get; set; }
		
		[JsonProperty("gif")]
		public string Gif { get; set; }
		
		[JsonProperty("poster")]
		public string Poster { get; set; }
		
		[JsonProperty("thumbnail")]
		public string Thumbnail { get; set; }
		
		[JsonProperty("vthumbnail")]
		public string VideoThumbnail { get; set; }
	}
}