using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class RedGifsTemporary 
	{
		[JsonProperty("token")]
		public string Token { get; set; }
		
		[JsonProperty("addr")]
		public string Address { get; set; }
		
		[JsonProperty("agent")]
		public string Agent { get; set; }
		
		[JsonProperty("rtfm")]
		public string RTFM { get; set; }
	}
}