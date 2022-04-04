using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mayfly.Akinator.Enumerations;
using Mayfly.Services;
using Mayfly.Services.Poll;
using Mayfly.Services.Trivia;

namespace Mayfly.Modules
{
	public class GameModule : MayflyModule
	{
		public AkinatorService akinator { get; set; }
		public RouletteService roulette { get; set; }
		public TriviaService trivia { get; set; }
		public PollService poll { get; set; }

		[SlashCommand("genie", "Akinator in discord.")]
		public async Task<RuntimeResult> Genie(ServerType type = ServerType.Person, Language language = Language.English)
		{
			if (!await akinator.NewSessionAsync(Context, language, type))
			{
				return MayflyResult.FromError("InstanceAlreadyCreated", "You are already talking to a genie.");
			}
			
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("roulette", "Classic Russian Roulette."), RequireContext(ContextType.Guild)]
		public async Task Roulette()
		{
			await roulette.Next(Context);
		}

		[SlashCommand("trivia", "Tower Unite's Trivia.")]
		public async Task<RuntimeResult> Trivia([MinValue(5), MaxValue(50)] int total = 10, TriviaCategory category = TriviaCategory.All, TriviaDifficulty difficulty = TriviaDifficulty.All, TriviaType type = TriviaType.All)
		{
			if (!await trivia.NewSession(Context, new TriviaOptions(total, category, difficulty, type)))
			{
				return MayflyResult.FromError("InstanceAlreadyExists", "You are already talking to a genie.");
			}
			
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("poll", "Simple poll system.")]
		public async Task<RuntimeResult> Poll(string title, string option1, string option2, string option3 = "", string option4 = "", string option5 = "", string option6 = "", string option7 = "", string option8 = "", string option9 = "", string option10 = "")
		{
			string[] options = { option1, option2, option3, option4, option5, option6, option7, option8, option9, option10 };
				
			if (!await poll.Create(Context, title, options.Where(x => !string.IsNullOrEmpty(x)).ToArray()))
			{
				return MayflyResult.FromUserError("OutOfRange", "Minimum of 2 options required, maximum of 10.");
			}
			
			return MayflyResult.FromSuccess();
		}
		
		
	}
}