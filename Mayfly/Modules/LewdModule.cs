using Discord;
using Discord.Interactions;
using Mayfly.Services;
using Mayfly.Structures;
using RuntimeResult = Discord.Interactions.RuntimeResult;

namespace Mayfly.Modules
{
	[RequireNsfw, Group("lewd", "Do you really need to ask?")]
	public class LewdModule : MayflyModule
	{
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }

		[SlashCommand("gelbooru", "Anime titties.")]
		public async Task<RuntimeResult> Gelbooru(string tags = "")
		{
			await DeferAsync();

			string safe = string.Join("+", tags.Split(' ').Select(Uri.EscapeDataString));
			GelbooruResult result = await Http.GetJsonAsync<GelbooruResult>($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=20&json=1&tags=sort:random+score:>=3+{safe}");

			if (result is { Posts.Count: > 0 })
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
						ImageUrl = post.FileUrl
					}.Build());
				}
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("EmptyResult", "Couldn't not find any images with provided tags.");
		}

		[SlashCommand("rule34", "Video game titties.")]
		public async Task<RuntimeResult> Rule34(string tags = "")
		{
			await DeferAsync();
			
			string safe = string.Join("+", tags.Split(' ').Select(Uri.EscapeDataString));
			Rule34Post[] posts = await Http.GetJsonAsync<Rule34Post[]>($"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&json=1&limit=20&tags=sort:random+{safe}");

			if (posts is { Length: > 0 })
			{
				Rule34Post post = Random.Pick(posts);
				
				if (post.FileUrl.EndsWith(".webm") || post.FileUrl.EndsWith(".mp4"))
				{
					await FollowupAsync(post.FileUrl);
				}
				else
				{
					await FollowupAsync(embed: new EmbedBuilder()
					{
						Color = Color.Blue,
						ImageUrl = post.FileUrl
					}.Build());
				}
				
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromUserError("EmptyResult", "Couldn't not find any images with provided tags.");
		}

		[SlashCommand("redgifs", "3D titties.")]
		public async Task<RuntimeResult> Redgifs()
		{
			await DeferAsync();
			RedgifsResult result = await Http.GetJsonAsync<RedgifsResult>($"https://api.redgifs.com/v2/gifs/search");

			if (result is { Total: > 0 })
			{
				await FollowupAsync(Random.Pick(result.Gifs).Urls.HD);
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromError("ServiceError", "Something went wrong!");
		}

		[SlashCommand("random", "All titties.")]
		public async Task<RuntimeResult> All(string tags = "")
		{
			return Random.Next(2) switch
			{
				0 => await Gelbooru(tags),
				1 => await Rule34(tags),
				2 => await Redgifs(),
				_ => MayflyResult.FromError("Unknown", "This legit shouldn't be possible.")
			};
		}
	}
}