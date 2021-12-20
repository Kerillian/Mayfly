using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Mayfly.Attributes.Parameter
{
	public class RangeAttribute : ParameterPreconditionAttribute
	{
		public readonly IComparable Min;
		public readonly IComparable Max;

		public RangeAttribute(int min, int max)
		{
			this.Min = min;
			this.Max = max;
		}

		public RangeAttribute(long min, long max)
		{
			this.Min = min;
			this.Max = max;
		}
		
		public RangeAttribute(float min, float max)
		{
			this.Min = min;
			this.Max = max;
		}
		
		public RangeAttribute(double min, double max)
		{
			this.Min = min;
			this.Max = max;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (value is IComparable c && c.CompareTo(Min) > -1 && c.CompareTo(Max) < 1)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			return Task.FromResult(PreconditionResult.FromError($"Parameter `{parameter.Name}` is out of range ({Min}..{Max})."));
		}
	}
}