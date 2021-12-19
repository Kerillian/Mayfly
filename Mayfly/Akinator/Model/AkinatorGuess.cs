using System;

namespace Mayfly.Akinator.Model
{
	public class AkinatorGuess
	{
		public ulong ID { get; set; }
		public string Name { get; }
		public string Description { get; }
		public float Probability { get; set; }
		public Uri PhotoPath { get; set; }

		public AkinatorGuess(string name, string description)
		{
			Name = name;
			Description = description;
		}
	}
}
