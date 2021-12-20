using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Attributes;
using Mayfly.Database;
using Mayfly.Services;

namespace Mayfly.Modules
{
	/*
	 * TODO: Finish inventory display
	 * TODO: Finish reminders system
	 */
	
	public class PersistentModule : MayflyModule
	{
		public DatabaseService Database { get; set; }

		[Command("profile"), Summary("Get profile data.")]
		public async Task<RuntimeResult> Profile()
		{
			UserData data = await Database.GetUserAsync(Context.User);

			if (data is null)
			{
				return MayflyResult.FromError("QueryException", "Something went horrifically wrong on my end, please try again.");
			}

			List<Embed> embeds = new List<Embed>
			{
				new EmbedBuilder()
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
				}.Build(),
			};

			// List<ItemData> items = await Database.GetItemsAsync(Context.User);
			// embeds.AddRange(items.Select(DatabaseService.ItemEmbed));

			await ReplyAsync(embeds: embeds.ToArray());
			return MayflyResult.FromSuccess();
		}

		[Hidden, RequireOwner]
		[Command("market"), Summary("List items in the market")]
		public async Task Market()
		{
			List<ItemData> items = await Database.GetItemsAsync();
			await ReplyAsync(embeds: items.Select(DatabaseService.ItemEmbed).ToArray());
		}

		[Hidden, RequireOwner]
		[Command("sell"), Summary("Sell your belongings to the market.")]
		public async Task Sell(string name, int amount = 1)
		{
			TransactionResponse response = await Database.SellItemAsync(Context.User, name, amount);
			
			await ReplyToAsync(response.Code switch
			{
				MarketCode.Air          => "You can't sell an empty bag.",
				MarketCode.TooExpensive => "Umm... how??",
				MarketCode.ItemNotFound => "I... i don't see what you're trying to sell me.",
				MarketCode.UserNotFound => "Sorry, but you don't seem to be in our system.",
				MarketCode.Success      => $"You sold `{response.Amount:N0}` `{name}` for ${response.Money:N0}",
				_                       => "Something went horribly wrong, please contact `Kerillian` as soon as possible."
			});
		}
		
		[Hidden, RequireOwner]
		[Command("purchase"), Alias("buy"), Summary("Purchase goods from the market.")]
		public async Task Purchase(string name, int amount = 1)
		{
			TransactionResponse response = await Database.PurchaseItemAsync(Context.User, name, amount);

			await ReplyToAsync(response.Code switch
			{
				MarketCode.Air          => "Umm.. Sorry that's not for sale.",
				MarketCode.TooExpensive => "So umm... your card declined, are you sure you have enough money for this?",
				MarketCode.ItemNotFound => "We don't seem to have that item in stock. Sorry.",
				MarketCode.UserNotFound => "I'm sorry but we can't do business, your ID is expired.",
				MarketCode.Success      => $"You purchased `{response.Amount:N0}` `{name}` for ${response.Money:N0}",
				_                       => "Something went horribly wrong, please contact `Kerillian` as soon as possible."
			});
		}
	}
}