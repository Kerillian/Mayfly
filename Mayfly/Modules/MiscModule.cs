using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class MiscModule : MayflyModule
	{
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }
		public WaifulabsService Waifulabs { get; set; }
		
		[SlashCommand("tpdne", "https://thispersondoesnotexist.com/")]
		public async Task TPDNE()
		{
			await DeferAsync();
			await using Stream stream = await Http.GetStreamAsync("https://thispersondoesnotexist.com/image");
			await FollowupWithFileAsync(stream, "person.jpg");
		}
		
		[SlashCommand("twdne", "https://www.thiswaifudoesnotexist.net/")]
		public async Task TWDNE()
		{
			await DeferAsync();
			
			if (Random.Chance())
			{
				await using Stream stream = await Http.GetStreamAsync($"https://www.thiswaifudoesnotexist.net/example-{Random.Next(0, 100000)}.jpg");
				await FollowupWithFileAsync(stream, "anime.jpg");
			}
			else
			{
				await using Stream stream = await Waifulabs.GetImageStream();
				await FollowupWithFileAsync(stream, "anime.png");
			}
		}

		[SlashCommand("inspire", "inspirobot.me in discord.")]
		public async Task Inspire()
		{
			await DeferAsync();
			string url = await Http.GetStringAsync("http://inspirobot.me/api?generate=true");

			await FollowupAsync(embed: new EmbedBuilder()
			{
				ImageUrl = url,
				Color = new Color(0x1B8809),
				Author = new EmbedAuthorBuilder()
				{
					IconUrl = "http://inspirobot.me/website/images/inspirobot-dark-green.png",
					Name = "Inspirobot.me",
					Url = "http://inspirobot.me/"
				}
			}.Build());
		}
	}
}