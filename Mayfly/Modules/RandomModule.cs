using Discord;
using Discord.Interactions;
using Mayfly.Services;
using Mayfly.Structures;
using Mayfly.UrlBuilders;
using Color = Discord.Color;

namespace Mayfly.Modules
{
	public class RandomModule : MayflyModule
	{
		private static readonly string[] DiceIds = new string[6] { "6Zzf1KL", "RKCQ1w5", "P2F567A", "cx94cKE", "zyGoZXz", "a9PEYtY" };
		private static readonly string[] Answers = new string[20]
		{
			"It is certain.",
			"It is decidedly so.",
			"Without a doubt.",
			"Yes – definitely.",
			"You may rely on it.",
			"As I see it, yes.",
			"Most likely.",
			"Outlook good.",
			"Yes.",
			"Signs point to yes.",
			"Reply hazy, try again.",
			"Ask again later.",
			"Better not tell you now.",
			"Cannot predict now.",
			"Concentrate and ask again.",
			"Don't count on it.",
			"My reply is no.",
			"My sources say no.",
			"Outlook not so good.",
			"Very doubtful."
		};
		
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }
		public BotConfig Config { get; set; }

		[SlashCommand("random", "Express yourself randomly.")]
		public async Task<RuntimeResult> RandomQuery(string query)
		{
			await DeferAsync();
			TenorSearch search = await Http.GetJsonAsync<TenorSearch>(new TenorSearchBuilder(Config.TenorKey).WithQuery(query).WithLimit(50).Build());

			if (search?.Results.Length > 0)
			{
				TenorGif gif = Random.Pick(search.Results);
				TenorFormats format = Random.Pick(gif.Media);

				await FollowupAsync(format.Gif.Url);
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("NoResults", "I couldn't find anything to express that, sorry.");
		}
		
		[SlashCommand("dice", "Roll a Six-sided die.")]
		public async Task Dice()
		{
			int roll = Random.Dice();
			string url = $"https://i.imgur.com/{DiceIds[roll - 1]}.png";

			await RespondAsync(embed: new EmbedBuilder()
			{
				Title = "Dice Roll",
				Description = $"You rolled a {roll}",
				Color = new Color(0x527BBB),
				ThumbnailUrl = url
			}.Build());
		}
		
		[SlashCommand("foretell", "Predicts the future or a future event.")]
		public async Task Foretell(string question)
		{
			await RespondAsync(Random.Pick(Answers));
		}
		
		[SlashCommand("rbytes", "Generate a file with random bytes.")]
		public async Task RandomBytes(string filename, [MinValue(1), MaxValue(8000000)] int size)
		{
			byte[] bytes = new byte[size];
			Random.NextBytes(bytes);

			await using Stream stream = new MemoryStream(bytes);
			await RespondWithFileAsync(stream, filename);
		}
		
		[SlashCommand("xkcd", "Gets a random comic from xkcd.com")]
		public async Task Xkcd()
		{
			string url = "https://xkcd.com/" + Random.Next(0, 2551);
			XkcdPost post = await Http.GetJsonAsync<XkcdPost>(url + "/info.0.json");

			await RespondAsync(embed: new EmbedBuilder()
			{
				Color = new Color(0x96A8C8),
				Timestamp = new DateTimeOffset(new DateTime(post.Year, post.Month, post.Day)),
				ImageUrl = post.Img,
				Author = new EmbedAuthorBuilder()
				{
					Name = post.SafeTitle,
					Url = url,
					IconUrl = "https://xkcd.com/s/0b7742.png"
				}
			}.Build());
		}
	}
}