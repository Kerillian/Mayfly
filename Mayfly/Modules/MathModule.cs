using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Mayfly.Services;
using Mayfly.Structures;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Mayfly.Modules
{
	public class MathModal : IModal
	{
		public string Title => "MathJS Console";

		[InputLabel("Expression")]
		[ModalTextInput("expression", TextInputStyle.Paragraph, "9 + 10")]
		public string Expression { get; set; }
	}
	
	public class MathModule : MayflyModule
	{
		private readonly HttpService http;

		public MathModule(DiscordSocketClient dsc, HttpService hs)
		{
			http = hs;
		}

		private List<string> GetValidLines(IEnumerable<string> lines)
		{
			return lines.Where(s => !string.IsNullOrWhiteSpace(s) && s != "undefined" && !s.StartsWith("#")).ToList();
		}

		[SlashCommand("math", "Calculate stuff.")]
		public async Task Calc()
		{
			await RespondWithModalAsync<MathModal>("math_modal");
		}

		[ModalInteraction("math_modal")]
		public async Task<RuntimeResult> ModalResponse(MathModal modal)
		{
			if (modal.Expression.Replace(" ", "") == "9+10")
			{
				await RespondAsync("21");
				return MayflyResult.FromSuccess();
			}

			string[] lines = modal.Expression.Split('\n');
			List<string> clean = GetValidLines(lines);

			MathJsResponse response = await http.PostJsonAsync<MathJsBody, MathJsResponse>("http://api.mathjs.org/v4/", new MathJsBody()
			{
				Expression = clean,
				Precision = 14
			});

			if (response == null)
			{
				return MayflyResult.FromError("OhNo", "Something went very wrong, please try again later.");
			}

			if (!string.IsNullOrEmpty(response.Error))
			{
				return MayflyResult.FromUserError("InvalidExpression", response.Error);
			}

			Queue<string> queue = new Queue<string>(response.Result);
			StringBuilder builder = new StringBuilder();

			foreach (string line in lines)
			{
				builder.AppendLine(line);

				if (clean.Any(c => line == c))
				{
					builder.AppendLine($"\t-> {queue.Dequeue()}");
				}
			}
			
			await RespondAsync(Format.Code(builder.ToString()));
			return MayflyResult.FromSuccess();
		}

		// [SlashCommand("math", "Calculate stuff.")]
		// public async Task<RuntimeResult> Calc(string expression)
		// {
		// 	await DeferAsync();
		// 	
		// 	if (expression.Replace(" ", "") == "9+10")
		// 	{
		// 		await RespondAsync("21");
		// 		return MayflyResult.FromSuccess();
		// 	}
		// 	
		// 	string calculated = await http.GetStringAsync("https://api.mathjs.org/v4/?expr=" + Uri.EscapeDataString(expression));
		// 	
		// 	if (calculated.Contains("Error"))
		// 	{
		// 		return MayflyResult.FromError("InvalidExpression", calculated.Replace("", "Error: "));
		// 	}
		// 	
		// 	await FollowupAsync(calculated);
		// 	return MayflyResult.FromSuccess();
		// }
	}
}