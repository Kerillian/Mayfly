using Newtonsoft.Json;

namespace Mayfly.Akinator.Model.External
{
	internal class Identification
	{
		[JsonProperty("channel")]
		public int Channel { get; set; }

		[JsonProperty("session")]
		public string Session { get; set; }

		[JsonProperty("signature")]
		public string Signature { get; set; }
	}
}