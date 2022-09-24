using Discord;
using Discord.Interactions;
using Mayfly.Services;
using Mayfly.Structures;
using Mayfly.UrlBuilders;
using RuntimeResult = Discord.Interactions.RuntimeResult;

namespace Mayfly.Modules
{
	[RequireNsfw, Group("lewd", "Do you really need to ask?")]
	public class LewdModule : MayflyModule
	{
		public HttpService Http { get; set; }
		public RandomService Random { get; set; }

		[SlashCommand("gelbooru", "Anime moment.")]
		public async Task<RuntimeResult> Gelbooru(string tags = "")
		{
			await DeferAsync();
			GelbooruResult result = await Http.GetJsonAsync<GelbooruResult>(new GelbooruPostsBuilder().WithTags($"sort:random+score:>=3+{tags}").WithLimit(20).Build());
			
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

		[SlashCommand("rule34", "If it exists...")]
		public async Task<RuntimeResult> Rule34(string tags = "")
		{
			await DeferAsync();
			Rule34Post[] posts = await Http.GetJsonAsync<Rule34Post[]>(new Rule34PostsBuilder().WithTags($"sort:random+{tags}").WithLimit(20).Build());
			
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

		[SlashCommand("redgifs", "90% of these are in fact, not red. False advertising imo.")]
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

		[SlashCommand("random", "Spin the wheel.")]
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