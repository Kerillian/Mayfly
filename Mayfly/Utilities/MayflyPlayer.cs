﻿using Discord;
using Discord.WebSocket;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Mayfly.Extensions;

namespace Mayfly.Utilities
{
	public class QueueInfo
	{
		public IUser User { get; }
		public ISocketMessageChannel Channel { get; }
		public IDiscordInteraction Interaction { get; set; }
		public IMessage? QueueMessage { get; set; }

		public QueueInfo(IUser user, ISocketMessageChannel channel)
		{
			User = user;
			Channel = channel;
		}
	}
	
	public class MayflyPlayer : VoteLavalinkPlayer
	{
		private ulong lastId;

		public override async Task OnTrackStartedAsync(TrackStartedEventArgs e)
		{
			if (State == PlayerState.Playing && CurrentTrack?.Context is QueueInfo info)
			{
				if (info.Interaction != null)
				{
					lastId = (await info.Interaction.FollowupAsync(embed: await CurrentTrack.GetEmbedAsync("Playing"))).Id;
				}
				else
				{
					lastId = (await info.Channel.SendMessageAsync(embed: await CurrentTrack.GetEmbedAsync("Playing"))).Id;
				}
				
				if (info.QueueMessage is not null)
				{
					await info.QueueMessage.DeleteAsync();
				}
			}
			
			await base.OnTrackStartedAsync(e);
		}

		public override async Task OnTrackEndAsync(TrackEndEventArgs e)
		{
			if (CurrentTrack?.Context is QueueInfo info && lastId != 0)
			{
				await info.Channel.DeleteMessageAsync(lastId);
				lastId = 0;
			}

			Task _ = Task.Run(async () =>
			{
				await Task.Delay(1000);
				if (State != PlayerState.Playing && Queue.IsEmpty)
				{
					await DisconnectAsync();
				}
			});

			await base.OnTrackEndAsync(e);
		}
	}
}