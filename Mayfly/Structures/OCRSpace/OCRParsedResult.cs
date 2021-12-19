using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class OCRParsedResult
	{
		[JsonProperty("TextOverlay")]
		public OCRTextOverlay TextOverlay { get; set; }

		[JsonProperty("TextOrientation")]
		public string TextOrientation { get; set; }

		[JsonProperty("FileParseExitCode")]
		public int FileParseExitCode { get; set; }

		[JsonProperty("ParsedText")]
		public string ParsedText { get; set; }

		[JsonProperty("ErrorMessage")]
		public string ErrorMessage { get; set; }

		[JsonProperty("ErrorDetails")]
		public string ErrorDetails { get; set; }
	}
}