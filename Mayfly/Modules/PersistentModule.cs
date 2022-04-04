using Discord;
using Discord.Interactions;
using Mayfly.Database;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class PersistentModule : MayflyModule
	{
		public DatabaseService Database { get; set; }

		[SlashCommand("profile", "Get profile data.")]
		public async Task<RuntimeResult> Profile()
		{
			UserData data = await Database.GetUserAsync(Context.User);

			if (data is null)
			{
				return MayflyResult.FromError("QueryException", "Something went horrifically wrong on my end, please try again.");
			}

			await RespondAsync(embed: new EmbedBuilder()
			{
				Title = Context.User.Username,
				ThumbnailUrl = Context.User.GetAvatarUrl(),
				Color = Color.Purple,
				Description = Format.Code(string.Join("\n", new string[]
				{
					$"Level  : {DatabaseService.CalculateLevel(data.Experience)}",
					$"Money  : ${data.Money:N0}",
					$"Tokens : {data.Tokens:N0}"
				}))
			}.Build());
			
			return MayflyResult.FromSuccess();
		}
	}
}