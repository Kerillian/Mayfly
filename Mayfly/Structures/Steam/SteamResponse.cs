using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class SteamResponse<T> where T : class
	{
		[JsonProperty("response")]
		public T Response { get; set; } 
	}
}