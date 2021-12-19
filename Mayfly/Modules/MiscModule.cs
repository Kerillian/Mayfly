using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class MiscModule : MayflyModule
	{
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }
		public WaifulabsService Waifulabs { get; set; }

		[Command("tpdne"), Summary("https://thispersondoesnotexist.com/")]
		public async Task TPDNE()
		{
			await using Stream stream = await this.Http.GetStreamAsync("https://thispersondoesnotexist.com/image");
			await this.ReplyFileAsync(stream, "person.jpg");
		}

		[Command("twdne"), Summary("https://www.thiswaifudoesnotexist.net/")]
		public async Task TWDNE()
		{
			if (Random.Chance())
			{
				await using Stream stream = await this.Http.GetStreamAsync($"https://www.thiswaifudoesnotexist.net/example-{this.Random.Next(0, 100000)}.jpg");
				await this.ReplyFileAsync(stream, "anime.jpg");
			}
			else
			{
				await using Stream stream = await Waifulabs.GetImageStream();
				await this.ReplyFileAsync(stream, "anime.png");
			}
		}

		[Command("robot"), Summary("Generate a robot based on your id.")]
		public async Task Robot()
		{
			await ReplyAsync("", false, new EmbedBuilder()
			{
				Color = new Color(0x9EDBF9),
				ImageUrl = $"https://robohash.org/{Context.User.Id}.png?set=set3&bgset=bg{Random.Next(0, 2)}"
			}.Build());
		}

		[Command("robot"), Summary("Generate a robot based on your id.")]
		public async Task Robot([Remainder] string data)
		{
			await ReplyAsync("", false, new EmbedBuilder()
			{
				Color = new Color(0x9EDBF9),
				ImageUrl = $"https://robohash.org/{Uri.EscapeDataString(data)}.png?set=set3&bgset=bg{Random.Next(0, 2)}"
			}.Build());
		}

		[Command("inspire"), Summary("inspirobot.me in discord.")]
		public async Task Inspire()
		{
			string url = await Http.GetStringAsync("http://inspirobot.me/api?generate=true");

			await ReplyAsync("", false, new EmbedBuilder()
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