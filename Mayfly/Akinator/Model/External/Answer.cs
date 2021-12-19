using Newtonsoft.Json;

namespace Mayfly.Akinator.Model.External
{
	internal class Answer
	{
		[JsonProperty("answer")]
		public string Text { get; set; }
	}
}