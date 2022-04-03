using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mayfly.Services;
using Mayfly.Structures;
using Mayfly.Utilities;

namespace Mayfly.Modules
{
	[RequireNsfw]
	public class LewdModule : MayflyInteraction
	{
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }
		
		[SlashCommand("lewd", "Do you really need to ask?")]
		public async Task<RuntimeResult> Gelbooru(string tags)
		{
			await DeferAsync();
			
			string safe = string.Join("+", tags.Split(" ").Select(Uri.EscapeDataString));
			GelbooruResult result = await Http.GetJsonAsync<GelbooruResult>($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=20&json=1&tags=sort:random+score:>=3+{safe}");

			if (result is not null && result.Posts.Count > 0)
			{
				GelbooruPost post = Random.Pick(result.Posts);
				
				if (post.FileUrl.EndsWith(".webm") || post.FileUrl.EndsWith(".mp4"))
				{
					await FollowupAsync(post.FileUrl);
				}
				else
				{
					await FollowupAsync(embed: new EmbedBuilder()
					{
						Color = Color.Blue,
						ImageUrl = post.FileUrl,
					}.Build());
				}
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("EmptyResult", "Couldn't not find any images with provided tags.");
		}
		
		[SlashCommand("redgif", "Again, do you need really need to ask?")]
		public async Task<RuntimeResult> Redgifs(string tags)
		{
			await DeferAsync();
			string search = string.Join(',', tags.Split(" ").Select(t => StringUtility.ToTitleCase(StringUtility.GetAlphaNumeric(t))));
			RedgifsResult result = await Http.GetJsonAsync<RedgifsResult>($"https://api.redgifs.com/v2/gifs/search");

			if (result is { Total: > 0 })
			{
				await FollowupAsync(Random.Pick(result.Gifs).Urls.HD);
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("EmptyResult", "Couldn't find any gifs with provided tags.");
		}
	}
}