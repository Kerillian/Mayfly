namespace Mayfly.UrlBuilders
{
	public class RealbooruBuilder : BaseGelbooruUrlBuilder
	{
		private const string Endpoint = "https://realbooru.com/index.php?page=dapi&s=post&q=index&json=1";
		
		public RealbooruBuilder() : base(Endpoint)
		{
			
		}
	}
}