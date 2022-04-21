namespace Mayfly.UrlBuilders
{
	public abstract class BaseGelbooruUrlBuilder : BaseUrlBuilder
	{
		protected BaseGelbooruUrlBuilder(string url) : base(url)
		{
			IsFirst = false;
		}
		
		public BaseGelbooruUrlBuilder WithLimit(int limit)
		{
			if (limit is > 100 or < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(limit));
			}
			
			AppendParameter("limit", limit);
			return this;
		}

		public BaseGelbooruUrlBuilder WithPostId(int pid)
		{
			if (0 > pid)
			{
				throw new ArgumentOutOfRangeException(nameof(pid));
			}
			
			AppendParameter("pid", pid);
			return this;
		}

		public BaseGelbooruUrlBuilder WithTags(string tags)
		{
			AppendParameter("tags", string.Join("+", tags.Split(' ').Select(Uri.EscapeDataString)));
			return this;
		}

		public BaseGelbooruUrlBuilder WithChangeId(int cid)
		{
			if (0 > cid)
			{
				throw new ArgumentOutOfRangeException(nameof(cid));
			}
			
			AppendParameter("cid", cid);
			return this;
		}
		
		public BaseGelbooruUrlBuilder WithId(int id)
		{
			if (0 > id)
			{
				throw new ArgumentOutOfRangeException(nameof(id));
			}
			
			AppendParameter("id", id);
			return this;
		}
	}
}