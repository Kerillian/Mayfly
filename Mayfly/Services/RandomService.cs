using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Mayfly.Services
{
	public class RandomService : Random
	{
		[Flags]
		public enum RandomStringTypes : ushort
		{
			AlphaLower = 0,
			AlphaUpper = 1,
			Numeric = 2,
			Symbols = 4
		}
		
		public const RandomStringTypes ALL = RandomStringTypes.AlphaLower | RandomStringTypes.AlphaUpper | RandomStringTypes.Numeric | RandomStringTypes.Symbols;
		private const string Alpha = "abcdefghijklmnopqrstuvwxyz";
		private const string Numeric = "1234567890";
		private const string Symbols = "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?`~";
		
		private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

		public override int Next(int min, int max)
		{
			byte[] bytes = new byte[4];
			rng.GetBytes(bytes);
			uint scale = BitConverter.ToUInt32(bytes, 0);

			return (int)(min + (max - min) * (scale / (uint.MaxValue + 1.0)));
		}

		public override int Next(int max)
		{
			return Next(0, max);
		}

		public float NextFloat()
		{
			byte[] bytes = new byte[4];
			rng.GetBytes(bytes);
			return BitConverter.ToSingle(bytes, 0);
		}
		
		public float NextFloat(float min, float max)
		{
			return Math.Clamp(NextFloat(), min, max);
		}

		public double NextDouble()
		{
			byte[] bytes = new byte[8];
			rng.GetBytes(bytes);
			return BitConverter.ToDouble(bytes, 0);
		}
		
		public double NextDouble(double min, double max)
		{
			return Math.Clamp(NextDouble(), min, max);
		}

		public T Pick<T>(T[] array)
		{
			return array[Next(0, array.Length - 1)];
		}

		public T Pick<T>(List<T> list)
		{
			return list[Next(0, list.Count - 1)];
		}

		public T Pick<T>(JArray array)
		{
			return array[Next(0, array.Count - 1)].Value<T>();
		}

		public Match Pick(MatchCollection collection)
		{
			return collection[Next(0, collection.Count - 1)];
		}

		public T Pick<T>(IEnumerable<T> enumerable)
		{
			int index = Next(0, enumerable.Count() - 1);
			return enumerable.ElementAt(index);
		}

		public bool Chance(int likelihood = 50)
		{
			return Sample() * 100 < likelihood;
		}

		public int Dice(int sides = 6)
		{
			return Next(1, sides);
		}

		public string Shuffle(string str)
		{
			return string.Concat(str.OrderBy(c => Next()));
		}

		public string String(int length, RandomStringTypes types)
		{
			string temp = "";

			if (types.HasFlag(RandomStringTypes.AlphaLower))
			{
				temp += Alpha;
			}
			
			if (types.HasFlag(RandomStringTypes.AlphaUpper))
			{
				temp += Alpha.ToUpper();
			}
			
			if (types.HasFlag(RandomStringTypes.Numeric))
			{
				temp += Numeric;
			}

			if (types.HasFlag(RandomStringTypes.Symbols))
			{
				temp += Symbols;
			}

			temp = Shuffle(temp);

			return string.Concat(Enumerable.Repeat(temp, length).Select(s => s[Next(s.Length)]));
		}
	}
}