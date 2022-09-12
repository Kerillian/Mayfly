using System.Text;
using System.Security.Cryptography;

namespace Mayfly.Utilities
{
	public static class HashUtility
	{
		public static string Md5(string str)
		{
			byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}

		public static string Sha1(string str)
		{
			byte[] bytes = SHA1.HashData(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}

		public static string Sha256(string str)
		{
			byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}

		public static string Sha512(string str)
		{
			byte[] bytes = SHA512.HashData(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}
	}
}