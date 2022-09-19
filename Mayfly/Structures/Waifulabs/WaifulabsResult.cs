using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class WaifulabsResult
	{
		[JsonProperty("newGirls")]
		public List<WaifulabsGirl> NewGirls { get; set; } 
	}
}