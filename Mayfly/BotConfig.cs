using System;

namespace Mayfly
{
	public class BotConfig
	{
		public string Token { get; set; }
		public ulong DebugID { get; set; }
		public string SoundcloudKey { get; set; }
		public string SpotifyId { get; set; }
		public string SpotifySecret { get; set; }
		public string OCRSpaceKey { get; set; }
		public string SteamKey { get; set; }
		public string NookipediaKey { get; set; }
		public string TenorKey { get; set; }
		public string LavaLinkIP { get; set; }
		public ushort LavaLinkPort { get; set; }
		public string LavaLinkPassword { get; set; }
	}
}