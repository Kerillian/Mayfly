using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MathJsResponse
	{
		[JsonProperty("result")]
		public List<string> Result { get; set; }

		[JsonProperty("error")]
		public string Error { get; set; }
	}
}