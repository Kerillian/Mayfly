using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Mayfly.Services;

namespace Mayfly.Modules
{
	public class HelpModule : MayflyModule
	{
		public CommandService commandService { get; set; }
		public HelpService help { get; set; }
		public PaginationService pagination { get; set; }

		[Command("help"), Summary("Display help menu."), Priority(100), Alias("soup")]
		public async Task Help()
		{
			await pagination.SendMessageAsync(Context.Channel, new PaginatedMessage(help.GetPages(), null, null, Context.User, new AppearanceOptions()
			{
				Timeout = TimeSpan.FromMinutes(5),
				Style = DisplayStyle.Minimal
			}));
		}

		[Command("help"), Summary("Display help page.")]
		[Remarks("Display help page.")]
		public async Task<RuntimeResult> Help(int page)
		{
			if (help.TryGetPage(page, out EmbedBuilder builder))
			{
				await ReplyAsync(embed: builder.Build());
			}
			else
			{
				return MayflyResult.FromError("QueryFailure", "Invalid page index");
			}
			
			return MayflyResult.FromSuccess();
		}

		[Command("help"), Summary("Display help for command.")]
		[Remarks("You can also search for Enum types like BlockType, and groups like 'music'.")]
		public async Task<RuntimeResult> Help([Remainder] string query)
		{
			SearchResult result = commandService.Search(Context, query);
			
			if (result.IsSuccess)
			{
				await ReplyCodeAsync(help.GetResultHelp(result), "cs");
			}
			else if (query == "groups")
			{
				await ReplyAsync("", false, new EmbedBuilder()
				{
					Title = query,
					Description = Format.Code(help.GetGroups(), ""),
					Color = Color.Green
				}.Build());
			}
			else if (help.TryGetPages(query, out IReadOnlyCollection<EmbedBuilder> test))
			{
				if (test.Count > 1)
				{
					await pagination.SendMessageAsync(Context.Channel, new PaginatedMessage(test, null, null, Context.User, new AppearanceOptions()
					{
						Timeout = TimeSpan.FromMinutes(5),
						Style = DisplayStyle.Minimal
					}));
				}
				else
				{
					await ReplyAsync(embed: test.First().Build());
				}
			}
			else if (help.TryGetEnum(query, out string data))
			{
				await ReplyAsync("", false, new EmbedBuilder()
				{
					Title = query,
					Description = Format.Code(data, ""),
					Color = Color.Green
				}.Build());
			}
			else
			{
				return MayflyResult.FromError("QueryFailure", $"Couldn't find any commands, pages, groups, or enums with query \"{query}\"");
			}

			return MayflyResult.FromSuccess();
		}
	}
}