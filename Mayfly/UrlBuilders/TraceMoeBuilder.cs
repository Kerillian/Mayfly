namespace Mayfly.UrlBuilders
{
	public class TraceMoeBuilder : BaseUrlBuilder
	{
		private const string Endpoint = "https://api.trace.moe/search?anilistInfo";

		public TraceMoeBuilder() : base(Endpoint)
		{
			IsFirst = false;
		}

		public TraceMoeBuilder WithUrl(string url)
		{
			AppendParameter("url", Uri.EscapeDataString(url));
			return this;
		}

		public TraceMoeBuilder WithCutBorders()
		{
			AppendParameter("cutBorders", 1);
			return this;
		}

		public TraceMoeBuilder WithAnilistId(int anilistId)
		{
			AppendParameter("anilistID", anilistId);
			return this;
		}
	}
}