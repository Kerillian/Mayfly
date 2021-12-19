using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class RedgifsGif
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		
		[JsonProperty("createDate")]
		public long CreateDate { get; set; }
		
		[JsonProperty("hasAudio")]
		public bool HasAudio { get; set; }
		
		[JsonProperty("width")]
		public uint Width { get; set; }
		
		[JsonProperty("height")]
		public uint Height { get; set; }
		
		[JsonProperty("likes")]
		public uint Likes { get; set; }
		
		[JsonProperty("tags")]
		public List<string> Tags { get; set; }
		
		[JsonProperty("verified")]
		public bool Verified { get; set; }
		
		[JsonProperty("views")]
		public uint Views { get; set; }
		
		[JsonProperty("duration")]
		public uint Duration { get; set; }
		
		[JsonProperty("published")]
		public bool Published { get; set; }
		
		[JsonProperty("urls")]
		public RedgifsUrls Urls { get; set; }
		
		[JsonProperty("userName")]
		public string UserName { get; set; }
		
		[JsonProperty("type")]
		public uint Type { get; set; }
		
		[JsonProperty("avgColor")]
		public string AvgColor { get; set; }
		
		[JsonProperty("gallery")]
		public object Gallery { get; set; }
	}
}