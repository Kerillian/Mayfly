using Newtonsoft.Json;

namespace Mayfly.Akinator.Model.External
{
	internal class BaseResponse<TParametersType> where TParametersType : IBaseParameters
	{
		[JsonProperty("completion")]
		public string Completion { get; set; }

		[JsonProperty("parameters")]
		public TParametersType Parameters { get; set; }
	}
}