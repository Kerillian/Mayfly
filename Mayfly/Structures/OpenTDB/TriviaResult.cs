using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TriviaResult
	{
		[JsonProperty("response_code")]
		public byte ResponseCode { get; set; }

		[JsonProperty("results")]
		public List<TriviaQuestion> Results { get; set; }
	}
}