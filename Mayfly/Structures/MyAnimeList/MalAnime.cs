using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MalAnime
	{
		[JsonProperty("mal_id")]
		public int MalId { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("images")]
		public MalAnimeImages Images { get; set; }

		[JsonProperty("trailer")]
		public MalAnimeTrailer Trailer { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("title_english")]
		public string TitleEnglish { get; set; }

		[JsonProperty("title_japanese")]
		public string TitleJapanese { get; set; }

		[JsonProperty("title_synonyms")]
		public string[] TitleSynonyms { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("source")]
		public string Source { get; set; }

		[JsonProperty("episodes")]
		public int? Episodes { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("airing")]
		public bool Airing { get; set; }
		
		[JsonProperty("aired")]
		public MalAnimeAired Aired { get; set; }

		[JsonProperty("duration")]
		public string Duration { get; set; }

		[JsonProperty("rating")]
		public string Rating { get; set; }

		[JsonProperty("score")]
		public float? Score { get; set; }

		[JsonProperty("scored_by")]
		public int? ScoredBy { get; set; }

		[JsonProperty("rank")]
		public int? Rank { get; set; }

		[JsonProperty("popularity")]
		public int? Popularity { get; set; }

		[JsonProperty("members")]
		public int? Members { get; set; }

		[JsonProperty("favorites")]
		public int? Favorites { get; set; }

		[JsonProperty("synopsis")]
		public string Synopsis { get; set; }

		[JsonProperty("background")]
		public string Background { get; set; }

		[JsonProperty("season")]
		public string Season { get; set; }

		[JsonProperty("year")]
		public int? Year { get; set; }

		[JsonProperty("broadcast")]
		public MalAnimeBroadcast Broadcast { get; set; }

		[JsonProperty("producers")]
		public MalAnimeExtra[] Producers { get; set; }

		[JsonProperty("licensors")]
		public MalAnimeExtra[] Licensors { get; set; }

		[JsonProperty("studios")]
		public MalAnimeExtra[] Studios { get; set; }

		[JsonProperty("genres")]
		public MalAnimeExtra[] Genres { get; set; }

		[JsonProperty("explicit_genres")]
		public MalAnimeExtra[] ExplicitGenres { get; set; }

		[JsonProperty("themes")]
		public MalAnimeExtra[] Themes { get; set; }

		[JsonProperty("demographics")]
		public MalAnimeExtra[] Demographics { get; set; }
	}
}