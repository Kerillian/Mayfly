using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TraceMoeResult
	{
		[JsonProperty("RawDocsCount")]
		public int RawDocsCount { get; set; } 

		[JsonProperty("RawDocsSearchTime")]
		public long RawDocsSearchTime { get; set; } 

		[JsonProperty("ReRankSearchTime")]
		public long ReRankSearchTime { get; set; } 

		[JsonProperty("CacheHit")]
		public bool CacheHit { get; set; } 

		[JsonProperty("trial")]
		public int Trial { get; set; }

		[JsonProperty("docs")]
		public List<TraceMoeDoc> Docs { get; set; } 

		[JsonProperty("limit")]
		public int Limit { get; set; } 

		[JsonProperty("limit_ttl")]
		public int LimitTtl { get; set; } 

		[JsonProperty("quota")]
		public int Quota { get; set; } 

		[JsonProperty("quota_ttl")]
		public int QuotaTtl { get; set; }
	}
}