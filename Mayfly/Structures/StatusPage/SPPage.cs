using System;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class SPPage
	{
		[JsonProperty("id")]
		public string Id; 

		[JsonProperty("name")]
		public string Name; 

		[JsonProperty("url")]
		public string Url; 

		[JsonProperty("time_zone")]
		public string TimeZone; 

		[JsonProperty("updated_at")]
		public DateTime UpdatedAt; 
	}
}