using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MinecraftProfile
	{
		[JsonProperty("name")]
		public string Name{ get; set; }

		[JsonProperty("id")]
		public string ID { get; set; }
	}
}