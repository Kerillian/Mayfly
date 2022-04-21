namespace Mayfly.UrlBuilders
{
	public class Rule34PostsBuilder : BaseGelbooruUrlBuilder
	{
		private const string Endpoint = "https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1";

		public Rule34PostsBuilder() : base(Endpoint)
		{
			
		}
	}
}