using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class RedgifsUser
	{
		[JsonProperty("creationtime")]
		public ulong CreationTime { get; set; }
		
		[JsonProperty("followers")]
		public uint Followers { get; set; }
		
		[JsonProperty("following")]
		public uint Following { get; set; }
		
		[JsonProperty("gifs")]
		public uint Gifs { get; set; }
		
		[JsonProperty("name")]
		public string Name { get; set; }
		
		[JsonProperty("profileImageUrl")]
		public string AvatarUrl { get; set; }
		
		[JsonProperty("profileUrl")]
		public string ProfileUrl { get; set; }
		
		[JsonProperty("publishedGifs")]
		public uint PublishedGifs { get; set; }
		
		[JsonProperty("subscription")]
		public uint Subscription { get; set; }
		
		[JsonProperty("url")]
		public string Url { get; set; }
		
		[JsonProperty("username")]
		public string Username { get; set; }
		
		[JsonProperty("verified")]
		public bool Verified { get; set; }
		
		[JsonProperty("views")]
		public ulong Views { get; set; }
		
		[JsonProperty("poster")]
		public string Poster { get; set; }
		
		[JsonProperty("preview")]
		public string Preview { get; set; }
		
		[JsonProperty("thumbnail")]
		public string Thumbnail { get; set; }
	}
}