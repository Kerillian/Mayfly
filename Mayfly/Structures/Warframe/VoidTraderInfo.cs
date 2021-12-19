using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class VoidTraderInfo
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		
		[JsonProperty("activation")]
		public DateTime Activation { get; set; }
		
		[JsonProperty("startString")]
		public string StartString { get; set; }

		[JsonProperty("expiry")]
		public DateTime Expiry { get; set; }
		
		[JsonProperty("active")]
		public bool Active { get; set; }
		
		[JsonProperty("character")]
		public string Character { get; set; }
		
		[JsonProperty("location")]
		public string Location { get; set; }
		
		[JsonProperty("inventory")]
		public List<VoidTraderItem> Inventory { get; set; }
	}
}