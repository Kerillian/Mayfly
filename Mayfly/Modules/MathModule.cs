using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Interactions;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class StackInfo
	{
		public readonly ulong Stacks;
		public readonly ulong Remainder;

		private StackInfo(ulong stacks, ulong remainder)
		{
			this.Stacks = stacks;
			this.Remainder = remainder;
		}

		public static StackInfo Calculate(ulong items, ulong stack)
		{
			return stack > items ? new StackInfo(0, items) : new StackInfo(items / stack, items % stack);
		}
	}

	public class MathModule : MayflyModule
	{
		public HttpService http { get; set; }

		private void WriteLine(Stack<string> lines, ulong num, string text)
		{
			if (1 > num)
			{
				return;
			}
			
			lines.Push(num > 1 ? $"{num} {text}s" : $"{num} {text}");
		}
		
		[SlashCommand("math", "Calculate stuff.")]
		public async Task<RuntimeResult> Calc(string expression)
		{
			await DeferAsync();
			
			if (expression.Replace(" ", "") == "9+10")
			{
				await RespondAsync("21");
				return MayflyResult.FromSuccess();
			}
		
			string calculated = await http.GetStringAsync("https://api.mathjs.org/v4/?expr=" + Uri.EscapeDataString(expression));

			if (calculated.Contains("Error"))
			{
				return MayflyResult.FromError("InvalidExpression", calculated.Replace("", "Error: "));
			}

			await FollowupAsync(calculated);
			return MayflyResult.FromSuccess();
		}
		
		[SlashCommand("stack", "Calculate item stacks.")]
		public async Task Stack(ulong items, ulong size = 64)
		{
			Stack<string> lines = new Stack<string>();
			StackInfo chests = StackInfo.Calculate(items, 27 * size);
			StackInfo stacks = StackInfo.Calculate(chests.Remainder, size);

			WriteLine(lines, chests.Stacks, "chest");
			WriteLine(lines, stacks.Stacks, "stack");
			WriteLine(lines, stacks.Remainder, "item");

			if (lines.Count > 1)
			{
				lines.Push("and " + lines.Pop());
			}

			await RespondAsync(string.Join(", ", lines.Reverse()));
		}
	}
}