using System;

namespace Mayfly.Attributes
{
	public class CreditAttribute : Attribute
	{
		public string Credit { get; set; }

		public CreditAttribute(string credit)
		{
			this.Credit = credit;
		}
	}
}