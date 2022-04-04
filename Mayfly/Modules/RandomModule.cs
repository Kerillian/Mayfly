using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mayfly.Extensions;
using Mayfly.Services;
using Mayfly.Structures;

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
		
		[SlashCommand("random", "Get a random image from imgur sub.")]
		public async Task<RuntimeResult> RandomSub(string sub)
		{
			await DeferAsync();
			ImgurResult resp = await Http.GetJsonAsync<ImgurResult>($"https://imgur.com/r/{Uri.EscapeDataString(sub)}/new.json");

			if (resp is { Success: true })
			{
				ImgurPost image = Random.Pick(resp.Data);

				if (image.Nsfw && Context.Channel is ITextChannel { IsNsfw: false })
				{
					return MayflyResult.FromError("NotNSFW", "This tag or image is marked NSFW, please use this tag in a NSFW channel.");
				}

				await FollowupAsync(embed: new EmbedBuilder()
				{
					Color = new Color(27, 183, 110),
					ImageUrl = "https://i.imgur.com/" + image.Hash + image.Ext,
					Timestamp = image.Timestamp,

					Author = new EmbedAuthorBuilder()
					{
						Name = image.Title,
						IconUrl = "https://s.imgur.com/images/favicon-32x32.png"
					},

					Footer = new EmbedFooterBuilder()
					{
						Text = image.Author
					}
				}.WithEmpty().Build());
			}

			return MayflyResult.FromSuccess();
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
			await this.RespondWithFileAsync(stream, filename);
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