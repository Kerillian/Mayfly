using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class VoidTraderItem
	{
		[JsonProperty("item")]
		public string Item { get; set; }
		
		[JsonProperty("ducats")]
		public uint Ducats { get; set; }
		
		[JsonProperty("credits")]
		public uint Credits { get; set; }
	}
}