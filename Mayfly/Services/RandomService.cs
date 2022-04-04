using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Mayfly.Services
{
	public class RandomService : Random
	{
		private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
		private const string Alpha = "abcdefghijklmnopqrstuvwxyz";
		private const string Numeric = "1234567890";
		private const string Symbols = "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?`~";

		public override int Next(int min, int max)
		{
			byte[] bytes = new byte[4];
			this.rng.GetBytes(bytes);
			uint scale = BitConverter.ToUInt32(bytes, 0);

			return (int)(min + (max - min) * (scale / (uint.MaxValue + 1.0)));
		}

		public override int Next(int max)
		{
			return this.Next(0, max);
		}

		public float NextFloat()
		{
			byte[] bytes = new byte[4];
			this.rng.GetBytes(bytes);
			return BitConverter.ToSingle(bytes, 0);
		}
		
		public float NextFloat(float min, float max)
		{
			return Math.Clamp(NextFloat(), min, max);
		}

		public double NextDouble()
		{
			byte[] bytes = new byte[8];
			this.rng.GetBytes(bytes);
			return BitConverter.ToDouble(bytes, 0);
		}
		
		public double NextDouble(double min, double max)
		{
			return Math.Clamp(this.NextDouble(), min, max);
		}

		public T Pick<T>(T[] array)
		{
			return array[this.Next(0, array.Length - 1)];
		}

		public T Pick<T>(List<T> list)
		{
			return list[this.Next(0, list.Count - 1)];
		}

		public T Pick<T>(JArray array)
		{
			return array[this.Next(0, array.Count - 1)].Value<T>();
		}

		public Match Pick(MatchCollection collection)
		{
			return collection[this.Next(0, collection.Count - 1)];
		}

		public T Pick<T>(IEnumerable<T> enumerable)
		{
			int index = this.Next(0, enumerable.Count() - 1);
			return enumerable.ElementAt(index);
		}

		public bool Chance(int likelihood = 50)
		{
			return this.Sample() * 100 < likelihood;
		}

		public int Dice(int sides = 6)
		{
			return this.Next(1, sides);
		}

		public string String(int length)
		{
			return string.Concat(Enumerable.Repeat(Alpha + Numeric + Symbols + Alpha.ToUpper(), length).Select(s => s[this.Next(s.Length)]));
		}
		
		public string AlphaString(int length)
		{
			return string.Concat(Enumerable.Repeat(Alpha, length).Select(s => s[this.Next(s.Length)]));
		}

		public string NumericString(int length)
		{
			return string.Concat(Enumerable.Repeat(Numeric, length).Select(s => s[this.Next(s.Length)]));
		}
			
		public string AlphanumericString(int length)
		{
			return string.Concat(Enumerable.Repeat(Alpha + Numeric, length).Select(s => s[this.Next(s.Length)]));
		}
	}
}