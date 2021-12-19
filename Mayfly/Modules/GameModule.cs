using System.Threading.Tasks;
using Discord.Commands;
using Mayfly.Akinator.Enumerations;
using Mayfly.Attributes.Parameter;
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

		[Command("genie"), Summary("Akinator in discord.")]
		[Remarks("🇾 = Yes\n🇳 = No\n🤔 = I don't know\n🔄 = Redo\n🚫 = Extinguish lamp")]
		public async Task<RuntimeResult> Genie(ServerType type = ServerType.Person, Language language = Language.English)
		{
			if (!await akinator.NewSessionAsync(Context.User, Context.Channel, language, type))
			{
				return MayflyResult.FromError("InstanceAlreadyCreated", "You are already talking to a genie.");
			}
			
			return MayflyResult.FromSuccess();
		}

		[Command("roulette"), Summary("Classic Russian Roulette."), RequireContext(ContextType.Guild)]
		[Remarks("Click means the firing pin hit an empty chamber.\nBANG means the gun has gone off.\nWhiz means the gun has gone off, but missed.")]
		public async Task Roulette()
		{
			await roulette.Next(Context);
		}

		[Command("trivia"), Summary("Tower Unite's Trivia.")]
		public async Task<RuntimeResult> Trivia([Range(5, 50)] int total = 10, TriviaCategory category = TriviaCategory.All, TriviaDifficulty difficulty = TriviaDifficulty.All, TriviaType type = TriviaType.All)
		{
			if (!await trivia.NewSession(Context, new TriviaOptions(total, category, difficulty, type)))
			{
				return MayflyResult.FromError("InstanceAlreadyExists", "You are already talking to a genie.");
			}
			
			return MayflyResult.FromSuccess();
		}

		[Command("poll"), Summary("Simple poll system.")]
		public async Task<RuntimeResult> Poll(string title, params string[] options)
		{
			if (!await poll.Create(Context.Channel, title, options))
			{
				return MayflyResult.FromUserError("OutOfRange", "Minimum of 2 options required, maximum of 20.");
			}
			
			return MayflyResult.FromSuccess();
		}
	}
}