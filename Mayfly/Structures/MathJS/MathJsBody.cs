using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class MathJsBody
	{
		[JsonProperty("expr")]
		public List<string> Expression { get; set; }

		[JsonProperty("precision")]
		public int? Precision { get; set; }
	}
}