using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class SteamPlayerSummaries
	{
		[JsonProperty("players")]
		public List<SteamPlayer> Players;
	}
}