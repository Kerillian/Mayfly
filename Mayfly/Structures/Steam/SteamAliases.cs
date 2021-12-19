using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class SteamAliases
	{
		[JsonProperty("newname")]
		public string NewName { get; set; }
		
		[JsonProperty("timechanged")]
		public string TimeChanged { get; set; }
	}
}