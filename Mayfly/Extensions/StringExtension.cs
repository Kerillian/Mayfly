using System.Text.RegularExpressions;

namespace Mayfly.Extensions
{
	public static class StringExtension
	{
		public static string Match(this string str, Regex pattern)
		{
			Match match = pattern.Match(str);

			return match.Groups.Count > 1 ? match.Groups[1].Value : null;
		}

		public static string LazySubstring(this string str, int startIndex, int length)
		{
			return str.Length > startIndex + length ? str.Substring(startIndex, length) : str;
		}
	}
}