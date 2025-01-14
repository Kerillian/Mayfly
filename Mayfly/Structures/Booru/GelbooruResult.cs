using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class GelbooruPost
	{
		[JsonProperty("source")]
		public string Source { get; set; }

		[JsonProperty("directory")]
		public string Directory { get; set; }

		[JsonProperty("hash")]
		public string Hash { get; set; }

		[JsonProperty("height")]
		public int Height { get; set; }

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("image")]
		public string Image { get; set; }

		[JsonProperty("change")]
		public int Change { get; set; }

		[JsonProperty("owner")]
		public string Owner { get; set; }

		[JsonProperty("parent_id")]
		public int? ParentId { get; set; }

		[JsonProperty("rating")]
		public string Rating { get; set; }

		[JsonProperty("sample")]
		public bool Sample { get; set; }

		[JsonProperty("sample_height")]
		public int SampleHeight { get; set; }

		[JsonProperty("sample_width")]
		public int SampleWidth { get; set; }

		[JsonProperty("score")]
		public int Score { get; set; }

		[JsonProperty("tags")]
		public string Tags { get; set; }

		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonProperty("file_url")]
		public string FileUrl { get; set; }

		[JsonProperty("created_at")]
		public string CreatedAt { get; set; }

		[JsonIgnore]
		public string[] TagArray => Tags.Split(' ');
	}

	public class GelbooruAttributes
	{
		[JsonProperty("limit")]
		public int Limit { get; set; }
		
		[JsonProperty("offset")]
		public int Offset { get; set; }
		
		[JsonProperty("count")]
		public int Count { get; set; }
	}
	
	public class GelbooruResult
	{
		[JsonProperty("@attributes")]
		public GelbooruAttributes Attributes { get; set; }

		[JsonProperty("post")]
		public List<GelbooruPost> Posts { get; set; }
	}
}