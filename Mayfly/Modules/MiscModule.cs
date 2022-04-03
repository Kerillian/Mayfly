using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class MiscModule : MayflyInteraction
	{
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }
		public WaifulabsService Waifulabs { get; set; }
		
		[SlashCommand("tpdne", "https://thispersondoesnotexist.com/")]
		public async Task TPDNE()
		{
			await DeferAsync();
			await using Stream stream = await this.Http.GetStreamAsync("https://thispersondoesnotexist.com/image");
			await this.FollowupWithFileAsync(stream, "person.jpg");
		}
		
		[SlashCommand("twdne", "https://www.thiswaifudoesnotexist.net/")]
		public async Task TWDNE()
		{
			await DeferAsync();
			
			if (Random.Chance())
			{
				await using Stream stream = await this.Http.GetStreamAsync($"https://www.thiswaifudoesnotexist.net/example-{this.Random.Next(0, 100000)}.jpg");
				await this.FollowupWithFileAsync(stream, "anime.jpg");
			}
			else
			{
				await using Stream stream = await Waifulabs.GetImageStream();
				await this.FollowupWithFileAsync(stream, "anime.png");
			}
		}
		
		[SlashCommand("robot", "Generate a robot based on your id.")]
		public async Task Robot()
		{
			await RespondAsync(embed: new EmbedBuilder()
			{
				Color = new Color(0x9EDBF9),
				ImageUrl = $"https://robohash.org/{Context.User.Id}.png?set=set3&bgset=bg{Random.Next(0, 2)}"
			}.Build());
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