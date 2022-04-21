using System.Text;

namespace Mayfly.UrlBuilders
{
	public abstract class BaseUrlBuilder
	{
		private StringBuilder Builder { get; } = new StringBuilder();
		protected bool IsFirst = true;

		protected static string GetEnumString<T>(T en)
		{
			return Enum.GetName(typeof(T), en);
		}
		
		protected BaseUrlBuilder(string url)
		{
			Builder.Append(url);
		}

		protected void AppendParameter(string parameter, object value)
		{
			if (IsFirst)
			{
				Builder.AppendFormat("?{0}={1}", parameter, value);
				IsFirst = false;
				return;
			}

			Builder.AppendFormat("&{0}={1}", parameter, value);
		}
		
		public string Build()
		{
			return Builder.ToString();
		}
		
		public override string ToString()
		{
			return Builder.ToString();
		}
	}
}