using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace Mayfly.Utilities
{
	public static class HashUtility
	{
		public static string Md5(string str)
		{
			using MD5 algorithm = MD5.Create();
			byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}

		public static string Sha1(string str)
		{
			using SHA1 algorithm = SHA1.Create();
			byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}

		public static string Sha256(string str)
		{
			using SHA256 algorithm = SHA256.Create();
			byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}

		public static string Sha512(string str)
		{
			using SHA512 algorithm = SHA512.Create();
			byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(str));
			return string.Concat(bytes.Select(b => b.ToString("X2")));
		}
	}
}