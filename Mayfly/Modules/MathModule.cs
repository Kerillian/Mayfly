using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.Interactions;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class MathModule : MayflyModule
	{
		public HttpService http { get; set; }

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
	}
}