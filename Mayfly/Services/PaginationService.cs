using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Mayfly.Services
{
	public enum StopAction
	{
		Clear,
		DeleteMessage
	}

	public enum DisplayStyle
	{
		Full,
		Minimal,
		Selector
	}

	public class PaginatedMessage
	{
		private string Title { get; }
		private Color EmbedColor { get; }
		private IReadOnlyCollection<Embed> Pages { get; }
		internal IUser User { get; }
		internal AppearanceOptions Options { get; }
		internal int CurrentPage { get; set; }
		internal int Count => this.Pages.Count;
		
		public PaginatedMessage(IEnumerable<EmbedBuilder> builders, string title = "", Color? embedColor = null, IUser user = null, AppearanceOptions options = null)
		{
			List<Embed> embeds = new List<Embed>();
			int i = 1;

			foreach (EmbedBuilder builder in builders)
			{
				builder.Title ??= title;
				builder.Color ??= embedColor ?? Color.Default;
				builder.Footer ??= new EmbedFooterBuilder().WithText($"Page {i++}/{builders.Count()}");
				embeds.Add(builder.Build());
			}

			Pages = embeds;
			Title = title;
			EmbedColor = embedColor ?? Color.Default;
			User = user;
			Options = options ?? new AppearanceOptions();
			CurrentPage = 1;
		}

		internal Embed GetEmbed()
		{
			return Pages.ElementAtOrDefault(CurrentPage - 1);
		}
	}

	public class AppearanceOptions
	{
		public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
		public DisplayStyle Style { get; set; } = DisplayStyle.Full;
		public StopAction OnStop { get; set; } = StopAction.Clear;
		public StopAction TimeoutAction { get; set; } = StopAction.Clear;
	}

	public class PaginationService
	{
		private readonly Dictionary<ulong, PaginatedMessage> messages;

		public PaginationService(DiscordSocketClient client)
		{
			this.messages = new Dictionary<ulong, PaginatedMessage>();
			client.ButtonExecuted += this.ButtonHandler;
		}

		public async Task<IUserMessage> SendMessageAsync(SocketInteractionContext ctx, PaginatedMessage paginated, bool followup = false)
		{
			IUserMessage message;

			if (paginated.Count > 1)
			{
				ComponentBuilder builder = new ComponentBuilder();
				
				switch (paginated.Options.Style)
				{
					case DisplayStyle.Full:
						builder.WithButton("First", "first", ButtonStyle.Secondary);
						builder.WithButton("Back", "back", ButtonStyle.Primary);
						builder.WithButton("Next", "next", ButtonStyle.Primary);
						builder.WithButton("Last", "first", ButtonStyle.Secondary);
						builder.WithButton("X", "stop", ButtonStyle.Danger);
						break;
					
					case DisplayStyle.Minimal:
						builder.WithButton("Back", "back", ButtonStyle.Primary);
						builder.WithButton("Next", "next", ButtonStyle.Primary);
						builder.WithButton("X", "stop", ButtonStyle.Danger);
						break;
					
					case DisplayStyle.Selector:
						builder.WithButton("Back", "back", ButtonStyle.Primary);
						builder.WithButton("Select", "select", ButtonStyle.Success);
						builder.WithButton("Next", "next", ButtonStyle.Primary);
						break;
				}

				if (followup)
				{
					message = await ctx.Interaction.FollowupAsync(embed: paginated.GetEmbed(), components: builder.Build());
				}
				else
				{
					await ctx.Interaction.RespondAsync(embed: paginated.GetEmbed(), components: builder.Build());
					message = await ctx.Interaction.GetOriginalResponseAsync();
				}
			}
			else
			{
				if (followup)
				{
					message = await ctx.Interaction.FollowupAsync(embed: paginated.GetEmbed());
				}
				else
				{
					await ctx.Interaction.RespondAsync(embed: paginated.GetEmbed());
					message = await ctx.Interaction.GetOriginalResponseAsync();
				}
				
				return message;
			}

			this.messages.Add(message.Id, paginated);

			if (paginated.Options.Timeout != TimeSpan.Zero)
			{
				Task _ = Task.Delay(paginated.Options.Timeout).ContinueWith(async _t =>
				{
					if (!this.messages.ContainsKey(message.Id))
					{
						return;
					}
					
					switch (paginated.Options.TimeoutAction)
					{
						case StopAction.DeleteMessage:
							await message.DeleteAsync();
							break;
						case StopAction.Clear:
							await message.RemoveAllReactionsAsync();
							break;
					}

					this.messages.Remove(message.Id);
				});
			}

			return message;
		}

		public async Task ButtonHandler(SocketMessageComponent component)
		{
			if (this.messages.TryGetValue(component.Message.Id, out PaginatedMessage page))
			{
				if (component.User.Id != page.User.Id)
				{
					return;
				}

				switch (component.Data.CustomId)
				{
					case "first":
						if (page.CurrentPage != 1)
						{
							page.CurrentPage = 1;
							await component.UpdateAsync(x => x.Embed = page.GetEmbed());
						}
						break;
					
					case "back":
						if (page.CurrentPage != 1)
						{
							page.CurrentPage--;
							await component.UpdateAsync(x => x.Embed = page.GetEmbed());
						}
						break;
					
					case "next":
						if (page.CurrentPage != page.Count)
						{
							page.CurrentPage++;
							await component.UpdateAsync(x => x.Embed = page.GetEmbed());
						}
						break;
					
					case "last":
						if (page.CurrentPage != page.Count)
						{
							page.CurrentPage = page.Count;
							await component.UpdateAsync(x => x.Embed = page.GetEmbed());
						}
						break;
					
					case "stop":
						switch (page.Options.OnStop)
						{
							case StopAction.DeleteMessage:
								await component.Message.DeleteAsync();
								break;
							
							case StopAction.Clear:
								await component.UpdateAsync(x => x.Components = null);
								break;
						}

						this.messages.Remove(component.Message.Id);
						break;
					
					case "select":
						await component.UpdateAsync(x => x.Components = null);
						this.messages.Remove(component.Message.Id);
						break;
				}
			}
		}
	}
}