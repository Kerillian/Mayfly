using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class AnilistTitle
	{
		[JsonProperty("native")]
		public string Native { get; set; }
		
		[JsonProperty("romaji")]
		public string Romaji { get; set; }
		
		[JsonProperty("english")]
		public string English { get; set; }
	}
	
	public class AnilistInfo
	{
		[JsonProperty("id")]
		public int Id { get; set; }
		
		[JsonProperty("idMal")]
		public int MyAnimeListId { get; set; }
		
		[JsonProperty("title")]
		public AnilistTitle Title { get; set; }
		
		[JsonProperty("synonyms")]
		public List<string> Synonyms { get; set; }
		
		[JsonProperty("isAdult")]
		public bool IsAdult { get; set; }
	}
	
	public class TraceMoeResult
	{
		[JsonProperty("anilist")]
		public AnilistInfo Info { get; set; }

		[JsonProperty("filename")]
		public string Filename { get; set; }
		
		[JsonProperty("episode")]
		public int Episode { get; set; }
		
		[JsonProperty("from")]
		public float From { get; set; }
		
		[JsonProperty("to")]
		public float To { get; set; }
		
		[JsonProperty("similarity")]
		public float Similarity { get; set; }
		
		[JsonProperty("video")]
		public string Video { get; set; }
		
		[JsonProperty("image")]
		public string Image { get; set; }
	}
	
	public class TraceMoeResponse
	{
		[JsonProperty("frameCount")]
		public int FrameCount;
		
		[JsonProperty("error")]
		public string Error;
		
		[JsonProperty("result")]
		public List<TraceMoeResult> Results;
	}
}