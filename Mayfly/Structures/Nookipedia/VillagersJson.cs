using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class VillagerJson
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		
		[JsonProperty("url")]
		public string Url { get; set; }
		
		[JsonProperty("alt_name")]
		public string AltName { get; set; }
		
		[JsonProperty("title_color")]
		public string TitleColor { get; set; }
		
		[JsonProperty("text_color")]
		public string TextColor { get; set; }
		
		[JsonProperty("id")]
		public string Id { get; set; }
		
		[JsonProperty("ImageUrl")]
		public string ImageUrl { get; set; }
		
		[JsonProperty("species")]
		public string Species { get; set; }
		
		[JsonProperty("personality")]
		public string Personality { get; set; }
		
		[JsonProperty("Gender")]
		public string Gender { get; set; }
		
		[JsonProperty("birthday_month")]
		public string BirthdayMonth { get; set; }
		
		[JsonProperty("birthday_day")]
		public string BirthdayDay { get; set; }
		
		[JsonProperty("sign")]
		public string Sign { get; set; }
		
		[JsonProperty("quote")]
		public string Quote { get; set; }
		
		[JsonProperty("phrase")]
		public string Phrase { get; set; }
		
		[JsonProperty("clothing")]
		public string Clothing { get; set; }
		
		[JsonProperty("islander")]
		public string Islander { get; set; }
		
		[JsonProperty("debut")]
		public string Debut { get; set; }
		
		[JsonProperty("prev_phrases")]
		public List<string> PreviousPhrases { get; set; }
		
		[JsonProperty("appearances")]
		public List<string> Appearances { get; set; }
	}
}