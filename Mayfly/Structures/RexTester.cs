using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class RexTester
	{
		[JsonProperty]
		public string Result { get; set; }
		
		[JsonProperty]
		public string Warnings { get; set; }
		
		[JsonProperty]
		public string Errors { get; set; }
		
		[JsonProperty]
		public string Stats { get; set; }
		
		[JsonProperty]
		public string[] Files { get; set; }
	}
}