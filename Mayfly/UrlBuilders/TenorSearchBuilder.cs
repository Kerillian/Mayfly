namespace Mayfly.UrlBuilders
{
	public enum TenorContentFilter
	{
		Off,
		Low,
		Medium,
		High
	}

	public enum TenorMediaFilter
	{
		Basic,
		Minimal
	}

	public enum TenorArRange
	{
		All,
		Wide,
		Standard
	}
	
	public class TenorSearchBuilder : BaseUrlBuilder
	{
		private const string Endpoint = "https://g.tenor.com/v1/search";

		public TenorSearchBuilder(string key) : base(Endpoint)
		{
			AppendParameter("key", Uri.EscapeDataString(key));
		}

		public TenorSearchBuilder WithQuery(string query)
		{
			AppendParameter("q", Uri.EscapeDataString(query));
			return this;
		}

		public TenorSearchBuilder WithLocale(string xx_YY)
		{
			AppendParameter("locale", Uri.EscapeDataString(xx_YY));
			return this;
		}
		
		public TenorSearchBuilder WithContentFilter(TenorContentFilter filter)
		{
			AppendParameter("contentfilter", GetEnumString(filter).ToLower());
			return this;
		}
		
		public TenorSearchBuilder WithMediaFilter(TenorMediaFilter filter)
		{
			AppendParameter("media_filter", GetEnumString(filter).ToLower());
			return this;
		}

		public TenorSearchBuilder WithArRange(TenorArRange range)
		{
			AppendParameter("ar_range", GetEnumString(range).ToLower());
			return this;
		}

		public TenorSearchBuilder WithLimit(int limit)
		{
			if (limit is > 50 or < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(limit));
			}
			
			AppendParameter("limit", limit);
			return this;
		}

		public TenorSearchBuilder WithPos(int pos)
		{
			if (0 > pos)
			{
				throw new ArgumentOutOfRangeException(nameof(pos));
			}
			
			AppendParameter("pos", pos);
			return this;
		}
		
		public TenorSearchBuilder WithAnonId(string id)
		{
			AppendParameter("anon_id", id);
			return this;
		}
	}
}