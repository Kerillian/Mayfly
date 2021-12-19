using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class SPStatus
	{
		[JsonProperty("indicator")]
		public string Indicator; 

		[JsonProperty("description")]
		public string Description; 
	}
}