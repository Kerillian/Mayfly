using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class OCRSpaceResult
	{
		[JsonProperty("ParsedResults")]
		public OCRParsedResult[] ParsedResults { get; set; }

		[JsonProperty("OCRExitCode")]
		public int ExitCode { get; set; }

		[JsonProperty("IsErroredOnProcessing")]
		public bool IsErroredOnProcessing { get; set; }

		[JsonProperty("ProcessingTimeInMilliseconds")]
		public int ProcessingTimeInMilliseconds { get; set; }

		[JsonProperty("SearchablePDFURL")]
		public string SearchablePDFURL { get; set; }
	}
}