using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Services;
using Mayfly.Structures;
using Mayfly.Utilities;

namespace Mayfly.Modules
{
	[RequireNsfw]
	public class LewdModule : MayflyModule
	{
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }

		[Command("lewd"), Summary("Do you really need to ask?")]
		[Remarks("https://gelbooru.com/index.php?page=tags&s=list&tags=*&sort=desc&order_by=index_count")]
		public async Task<RuntimeResult> Gelbooru(params string[] tags)
		{
			string safe = string.Join("+", tags.Select(Uri.EscapeDataString));
			GelbooruResult result = await Http.GetJsonAsync<GelbooruResult>($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=20&json=1&tags=sort:random+score:>=3+{safe}");

			if (result is not null && result.Posts.Count > 0)
			{
				GelbooruPost post = Random.Pick(result.Posts);
				
				if (post.FileUrl.EndsWith(".webm") || post.FileUrl.EndsWith(".mp4"))
				{
					await ReplyAsync(post.FileUrl);
				}
				else
				{
					await ReplyAsync("", false, new EmbedBuilder()
					{
						Color = Color.Blue,
						ImageUrl = post.FileUrl,
					}.Build());
				}
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("EmptyResult", "Couldn't not find any images with provided tags.");
		}

		[Command("redgif"), Summary("Again, do you really need to ask?")]
		[Remarks("https://www.redgifs.com/categories")]
		public async Task<RuntimeResult> Redgifs(params string[] tags)
		{
			string search = string.Join(',', tags.Select(t => StringUtility.ToTitleCase(StringUtility.GetAlphaNumeric(t))));
			//RedgifsResult result = await Http.GetJsonAsync<RedgifsResult>($"https://api.redgifs.com/v2/gifs/search?search_text={search}&count=100");
			
			// Redgifs broke searching somehow? and the changes didn't even get documented.
			RedgifsResult result = await Http.GetJsonAsync<RedgifsResult>($"https://api.redgifs.com/v2/gifs/search");

			if (result is { Total: > 0 })
			{
				await ReplyAsync(Random.Pick(result.Gifs).Urls.HD);
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("EmptyResult", "Couldn't find any gifs with provided tags.");
		}
	}
}