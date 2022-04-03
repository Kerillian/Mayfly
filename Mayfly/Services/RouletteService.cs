using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Mayfly.Extensions;

namespace Mayfly.Services
{
	public class RouletteClient
	{
		public byte CurrentChamber;
		public readonly byte ChamberedBullet;

		public RouletteClient(RandomService random)
		{
			this.CurrentChamber = (byte)random.Next(1, 6);
			this.ChamberedBullet = (byte)random.Next(1, 6);
		}
	}

	public class RouletteService
	{
		private readonly RandomService random;
		private readonly ConcurrentDictionary<(ulong, ulong), ulong[]> roleCache = new ConcurrentDictionary<(ulong, ulong), ulong[]>();
		private readonly ConcurrentDictionary<ulong, RouletteClient> clients = new ConcurrentDictionary<ulong, RouletteClient>();
		private readonly Embed clickEmbed = new EmbedBuilder().WithDescription("*Click*").WithColor(0x41C4F4).Build();
		private readonly Embed bangEmbed = new EmbedBuilder().WithDescription("**BANG**").WithColor(new Color(0xF44141)).Build();
		private readonly Embed whizEmbed = new EmbedBuilder().WithDescription("_**Whiz**_").WithColor(new Color(0xF4A641)).Build();

		public RouletteService(RandomService rs, DiscordSocketClient client)
		{
			this.random = rs;
			client.UserJoined += OnJoined;
		}

		private async Task OnJoined(SocketGuildUser user)
		{
			if (roleCache.TryRemove((user.Guild.Id, user.Id), out ulong[] roles))
			{
				await user.AddRolesAsync(roles);
			}
		}

		public async Task Next(SocketInteractionContext context)
		{
			if (context.User is not SocketGuildUser user)
			{
				return;
			}

			if (context.Channel is not SocketTextChannel channel)
			{
				return;
			}
			
			if (!this.clients.TryGetValue(context.Channel.Id, out RouletteClient client))
			{
				client = new RouletteClient(random);
				this.clients.TryAdd(context.Channel.Id, client);
			}

			if (client.CurrentChamber == client.ChamberedBullet)
			{
				try
				{
					if (context.Guild.CurrentUser.GetPermissions(channel).CreateInstantInvite)
					{
						if (user.IsKickable())
						{
							try
							{
								ulong[] roleIds = user.Roles.Select(r => r.Id).ToArray();
								
								await user.SendMessageAsync((await channel.CreateInviteAsync(0, 1)).Url);
								await user.KickAsync("Shot in the head during russian roulette.");
								await channel.SendMessageAsync(embed: bangEmbed);
								
								roleCache.AddOrUpdate((context.Guild.Id, user.Id), roleIds, (_, _) => roleIds);
							}
							catch
							{
								await context.Interaction.RespondAsync(embed: whizEmbed);
							}
						}
						else
						{
							await context.Interaction.RespondAsync(embed: whizEmbed);
						}
					}
					else
					{
						await context.Interaction.RespondAsync(embed: whizEmbed);
					}
				}
				catch
				{
					await context.Interaction.RespondAsync(embed: whizEmbed);
				}

				this.clients.TryRemove(context.Channel.Id, out RouletteClient _);
				return;
			}

			await context.Interaction.RespondAsync(embed: this.clickEmbed);

			if (++client.CurrentChamber > 6)
			{
				client.CurrentChamber = 1;
			}
		}
	}
}