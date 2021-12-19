using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class OCRTextOverlay
	{
		[JsonProperty("Lines")]
		public string[] Lines { get; set; }

		[JsonProperty("HasOverlay")]
		public bool HasOverlay { get; set; }

		[JsonProperty("Message")]
		public string Message { get; set; }
	}
}