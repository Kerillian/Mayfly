using Newtonsoft.Json;

namespace Mayfly.Akinator.Model.External
{
	internal class GuessElement
	{
		[JsonProperty("element")]
		public Character Character { get; set; }
	}
}