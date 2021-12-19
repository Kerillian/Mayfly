using Discord;

namespace Mayfly.Extensions
{
	public static class EmbedBuilderExtension
	{
		public static EmbedBuilder WithEmpty(this EmbedBuilder builder)
		{
			return builder.WithDescription("\u200B");
		}
	}
}