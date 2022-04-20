using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TenorMedia
	{
		[JsonProperty("preview")]
		public string Preview { get; set; }
		
		[JsonProperty("url")]
		public string Url { get; set; }
		
		[JsonProperty("dims")]
		public int[] Dims { get; set; }
		
		[JsonProperty("size")]
		public int Size { get; set; }
	}
}