using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class RedgifsResult
	{
		[JsonProperty("page")]
		public int Page { get; set; }
		
		[JsonProperty("pages")]
		public int Pages { get; set; }
		
		[JsonProperty("total")]
		public int Total { get; set; }

		[JsonProperty("gifs")]
		public List<RedgifsGif> Gifs { get; set; }
		
		[JsonProperty("users")]
		public List<RedgifsUser> Users { get; set; }
		
		[JsonProperty("tags")]
		public List<string> Tags { get; set; }

		[JsonIgnore]
		public bool Successful => Total > 0;
	}
}