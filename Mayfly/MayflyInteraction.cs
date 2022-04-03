using Discord;
using Discord.Interactions;

namespace Mayfly
{
	public class MayflyInteraction : InteractionModuleBase<SocketInteractionContext>
	{
		/*
		 * Respond
		 */
		
		protected async Task RespondSanitizedAsync(string text, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await this.Context.Interaction.RespondAsync(Format.Sanitize(text), embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
		}
		
		protected async Task RespondWithCodeAsync(string code, string lang = "", Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await this.Context.Interaction.RespondAsync(Format.Code(code, lang), embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
		}

		protected async Task RespondWithFileAsync(string path, string filename, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await Context.Interaction.RespondWithFileAsync(path, filename, text, embeds, isTTS, ephemeral, allowedMentions ?? AllowedMentions.None, components, embed, options ?? RequestOptions.Default);
		}

		protected async Task RespondWithFileAsync(Stream stream, string filename, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await Context.Interaction.RespondWithFileAsync(stream, filename, text, embeds, isTTS, ephemeral, allowedMentions ?? AllowedMentions.None, components, embed, options ?? RequestOptions.Default);
		}

		/*
		 * Followup
		 */
		
		protected async Task FollowupSanitizedAsync(string text, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await this.Context.Interaction.FollowupAsync(Format.Sanitize(text), embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
		}
		
		protected async Task FollowupWithCodeAsync(string code, string lang = "", Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await this.Context.Interaction.FollowupAsync(Format.Code(code, lang), embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
		}

		protected async Task FollowupWithFileAsync(string path, string filename, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await this.Context.Interaction.FollowupWithFileAsync(path, filename, text, embeds, isTTS, ephemeral, allowedMentions ?? AllowedMentions.None, components, embed, options ?? RequestOptions.Default);
		}
		
		protected async Task FollowupWithFileAsync(Stream stream, string filename, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
		{
			await this.Context.Interaction.FollowupWithFileAsync(stream, filename, text, embeds, isTTS, ephemeral, allowedMentions ?? AllowedMentions.None, components, embed, options ?? RequestOptions.Default);
		}
	}
}