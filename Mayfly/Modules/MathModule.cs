using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Commands;
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

		[Command("math"), Alias("calc"), Summary("Calculate stuff.")]
		public async Task<RuntimeResult> Calc([Remainder] string expression)
		{
			if (expression.Replace(" ", "") == "9+10")
			{
				await ReplyAsync("21");
				return MayflyResult.FromSuccess();
			}
		
			string calculated = await http.GetStringAsync("https://api.mathjs.org/v4/?expr=" + Uri.EscapeDataString(expression));

			if (calculated.Contains("Error"))
			{
				return MayflyResult.FromError("InvalidExpression", calculated.Replace("", "Error: "));
			}

			await ReplyAsync(calculated);
			return MayflyResult.FromSuccess();
		}

		[Command("stack"), Summary("Calculate item stacks.")]
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

			await ReplyAsync(string.Join(", ", lines.Reverse()));
		}
	}
}