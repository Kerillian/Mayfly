using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MinecraftNameHistory
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("changedToAt")]
		public long? ChangedToAt { get; set; }
	}
}