using Discord;

namespace Mayfly.Extensions
{
	public static class InteractionExtension
	{
		public static Task RespondOrFollowup(this IDiscordInteraction interaction, string text = null,
			Embed[] embeds = null,
			bool isTTS = false,
			bool ephemeral = false,
			AllowedMentions allowedMentions = null,
			MessageComponent components = null,
			Embed embed = null,
			RequestOptions options = null)
		{
			if (interaction.HasResponded)
			{
				return interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
			}

			return interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
		}
	}
}