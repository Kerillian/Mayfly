using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class SPResult
	{
		[JsonProperty("page")]
		public SPPage Page; 

		[JsonProperty("components")]
		public List<SPComponent> Components; 

		[JsonProperty("status")]
		public SPStatus Status; 
	}
}