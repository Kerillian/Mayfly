namespace Mayfly.UrlBuilders
{
	public class GelbooruPostsBuilder : BaseGelbooruUrlBuilder
	{
		private const string Endpoint = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1";

		public GelbooruPostsBuilder() : base(Endpoint)
		{
			
		}
	}
}