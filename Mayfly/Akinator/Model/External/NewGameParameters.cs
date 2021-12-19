using Newtonsoft.Json;

namespace Mayfly.Akinator.Model.External
{
	internal class NewGameParameters : IBaseParameters
	{
		[JsonProperty("identification")]
		internal Identification Identification { get; set; }

		[JsonProperty("step_information")]
		public Question StepInformation { get; set; }
	}
}