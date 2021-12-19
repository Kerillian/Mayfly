using System;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class UrbanPost
	{
		[JsonProperty("definition")]
		public string Definition { get; set; }

		[JsonProperty("permalink")]
		public string Permalink { get; set; }
		
		[JsonProperty("thumbs_up")]
		public int ThumbsUp { get; set; }

		[JsonProperty("sound_urls")]
		public string[] SoundUrls { get; set; }

		[JsonProperty("author")]
		public string Author { get; set; }

		[JsonProperty("word")]
		public string Word { get; set; }

		[JsonProperty("defid")]
		public int DefID { get; set; }

		[JsonProperty("current_vote")]
		public string CurrentVote { get; set; }

		[JsonProperty("written_on")]
		public DateTimeOffset WrittenOn { get; set; }

		[JsonProperty("example")]
		public string Example { get; set; }

		[JsonProperty("thumbs_down")]
		public int ThumbsDown { get; set; }
	}
}