using Mayfly.UrlBuilders;
using Xunit;

namespace Mayfly.Tests
{
	public class UrlBuilderTests
	{
		private const string TENOR_SEARCH = "https://g.tenor.com/v1/search?key=XXXXXXXXXXXX&limit=10&q=test";
		private const string GELBOORU_POSTS = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&tags=rating%3Asafe+red_eyes&limit=50";
		private const string RULE34_POSTS = "https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&tags=rating%3Asafe+blue_eyes&limit=50";
		private const string JIKAN_SEARCH = "https://api.jikan.moe/v4/anime?q=Spy%20x%20Family&limit=10&type=tv";
		private const string TRACE_MOE_SEARCH = "https://api.trace.moe/search?anilistInfo&url=https%3A%2F%2Fraw.githubusercontent.com%2Fsoruly%2Ftrace.moe%2Fmaster%2Fdemo.jpg";

		[Fact]
		public void Tenor()
		{
			Assert.Equal(TENOR_SEARCH, new TenorSearchBuilder("XXXXXXXXXXXX").WithLimit(10).WithQuery("test").Build());
		}
		
		[Fact]
		public void Gelbooru()
		{
			Assert.Equal(GELBOORU_POSTS, new GelbooruPostsBuilder().WithTags("rating:safe red_eyes").WithLimit(50).Build());
		}

		[Fact]
		public void Rule34()
		{
			Assert.Equal(RULE34_POSTS, new Rule34PostsBuilder().WithTags("rating:safe blue_eyes").WithLimit(50).Build());
		}

		[Fact]
		public void JikanSearch()
		{
			Assert.Equal(JIKAN_SEARCH, new JikanSearchBuilder().WithQuery("Spy x Family").WithLimit(10).WithType(JikanAnimeType.Tv).Build());
		}
		
		[Fact]
		public void TraceMoe()
		{
			Assert.Equal(TRACE_MOE_SEARCH, new TraceMoeBuilder().WithUrl("https://raw.githubusercontent.com/soruly/trace.moe/master/demo.jpg").Build());
		}
	}
}