using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class UrbanResult
	{
		[JsonProperty("list")]
		public UrbanPost[] List { get; set; }
	}
}