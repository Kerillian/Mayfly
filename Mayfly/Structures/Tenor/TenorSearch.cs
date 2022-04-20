using System.Text;
using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TenorSearch
	{
		[JsonProperty("results")]
		public TenorGif[] Results { get; set; }

		[JsonProperty("next")]
		public int Next { get; set; }

		[JsonIgnore]
		public bool HasNext => Next > 0;
	}

	public class TenorSearchBuilder
	{
		public enum ContentFilter
		{
			Off,
			Low,
			Medium,
			High
		}

		public enum MediaFilter
		{
			Basic,
			Minimal
		}

		public enum ArRange
		{
			All,
			Wide,
			Standard
		}
		
		public const string Endpoint = "https://g.tenor.com/v1/search";
		private StringBuilder builder = new StringBuilder();

		public TenorSearchBuilder(string key)
		{
			builder.Append(Endpoint).Append($"?key={Uri.EscapeDataString(key)}");
		}

		public TenorSearchBuilder WithQuery(string query)
		{
			builder.Append($"&q={Uri.EscapeDataString(query)}");
			return this;
		}

		public TenorSearchBuilder WithLocale(string xx_YY)
		{
			builder.Append($"&locale={Uri.EscapeDataString(xx_YY)}");
			return this;
		}
		
		public TenorSearchBuilder WithContentFilter(ContentFilter filter)
		{
			if (Enum.GetName(typeof(ContentFilter), filter) is { } str)
			{
				builder.Append($"&contentfilter={str.ToLower()}");
			}

			return this;
		}
		
		public TenorSearchBuilder WithMediaFilter(MediaFilter filter)
		{
			if (Enum.GetName(typeof(MediaFilter), filter) is { } str)
			{
				builder.Append($"&media_filter={str.ToLower()}");
			}

			return this;
		}

		public TenorSearchBuilder WithArRange(ArRange range)
		{
			if (Enum.GetName(typeof(ArRange), range) is { } str)
			{
				builder.Append($"&ar_range={str.ToLower()}");
			}

			return this;
		}

		public TenorSearchBuilder WithLimit(int limit)
		{
			if (limit is > 50 or < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(limit));
			}

			builder.Append($"&limit={limit}");
			return this;
		}

		public TenorSearchBuilder WithPos(int pos)
		{
			if (0 > pos)
			{
				throw new ArgumentOutOfRangeException(nameof(pos));
			}
			
			builder.Append($"&pos={pos}");
			return this;
		}
		
		public TenorSearchBuilder WithAnonId(string id)
		{
			builder.Append($"&anon_id={id}");
			return this;
		}

		public string Build()
		{
			return builder.ToString();
		}

		public override string ToString()
		{
			return builder.ToString();
		}
	}
}