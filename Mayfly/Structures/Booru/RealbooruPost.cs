using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class RealbooruPost
	{
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
		public long Change { get; set; }

		[JsonProperty("owner")]
		public string Owner { get; set; }

		[JsonProperty("parent_id")]
		public int? ParentId { get; set; }

		[JsonProperty("rating")]
		public string Rating { get; set; }

		[JsonProperty("sample")]
		public int? Sample { get; set; }

		[JsonProperty("sample_height")]
		public int? SampleHeight { get; set; }

		[JsonProperty("sample_width")]
		public int? SampleWidth { get; set; }

		[JsonProperty("score")]
		public int? Score { get; set; }

		[JsonProperty("tags")]
		public string Tags { get; set; }

		[JsonProperty("width")]
		public int Width { get; set; }

		[JsonIgnore]
		public string Url => $"https://realbooru.com//images/{Directory}/{Image}";
	}
}