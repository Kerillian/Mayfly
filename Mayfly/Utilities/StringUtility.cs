using System.Globalization;
using System.Linq;
using System.Text;

namespace Mayfly.Utilities
{
	public static class StringUtility
	{
		public static string GetProperName(string name)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];

				if (i > 0 && char.IsUpper(c))
				{
					sb.Append(' ');
					sb.Append(char.ToLowerInvariant(c));
				}
				else
				{
					sb.Append(c);
				}
			}

			return sb.ToString();
		}

		public static string GetAlphaNumeric(string str)
		{
			return string.Concat(str.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)));
		}
		
		public static string ToTitleCase(string str)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
		}
	}
}