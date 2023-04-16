using Discord;
using Discord.Interactions;
using Mayfly.Database;
using Mayfly.Services;
using Mayfly.Utilities;
using Color = Discord.Color;

namespace Mayfly.Modules
{
	public class PersistentModule : MayflyModule
	{
		public DatabaseService Database { get; set; }

		[SlashCommand("profile", "Get profile data.")]
		public async Task<RuntimeResult> Profile(IUser user = null)
		{
			user ??= Context.User;
			
			UserData data = await Database.GetUserAsync(user.Id);

			if (data is null)
			{
				return MayflyResult.FromError("QueryException", "No data for user.");
			}

			await RespondAsync(embed: new EmbedBuilder()
				.WithAuthor($"{Context.User.Username}#{Context.User.Discriminator}", Context.User.GetAvatarUrl(ImageFormat.Auto, 32))
				.WithColor(Color.Purple)
				.WithDescription(Format.Code(new TableBuilder("Level", "Money", "Tokens")
					.WithRow(DatabaseService.CalculateLevel(data.Experience).ToString(), $"${data.Money:N0}", $"{data.Tokens:N0}")
					.Build()))
				.Build());
			
			return MayflyResult.FromSuccess();
		}
	}
}