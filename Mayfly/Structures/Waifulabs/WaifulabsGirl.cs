using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class WaifulabsGirl
	{
		[JsonProperty("image")]
		public string Image { get; set; } 

		[JsonProperty("seeds")]
		public List<object> Seeds { get; set; } 
	}
}