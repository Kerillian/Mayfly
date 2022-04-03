using System;

namespace Mayfly
{
	public class BotConfig
	{
		public string Token { get; set; }
		public string Prefix { get; set; }
		public ulong DebugID { get; set; }
		public string SoundcloudKey { get; set; }
		public string OCRSpaceKey { get; set; }
		public string SteamKey { get; set; }
		public string LavaLinkIP { get; set; }
		public ushort LavaLinkPort { get; set; }
		public string LavaLinkPassword { get; set; }
	}
}