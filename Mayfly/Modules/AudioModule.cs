using Mayfly.Extensions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Filters;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Mayfly.Services;
using Mayfly.Utilities;

namespace Mayfly.Modules
{
	[RequireContext(ContextType.Guild), Group("music", "Music stuff.")]
	public class AudioModule : MayflyModule
	{
		public IAudioService LavaNode { private get; set; }
		public PaginationService Pagination { private get; set; }
		public RandomService RandomService { private get; set; }
		
		private async ValueTask<(MayflyPlayer, MayflyResult)> GetPlayer(bool autoConnect = false)
		{
			if (Context.User is not IGuildUser user || user.VoiceChannel is null)
			{
				return (null, MayflyResult.FromError("NoVoiceChannel", "User is not in a voice channel."));
			}

			MayflyPlayer player = LavaNode.GetPlayer<MayflyPlayer>(Context.Guild.Id);

			if (player?.VoiceChannelId.HasValue == false)
			{
				player.Dispose();
				player = null;
			}

			if (player is null)
			{
				if (autoConnect)
				{
					if (Context.Guild.CurrentUser.GetPermissions(user.VoiceChannel) is not {Connect: true, Speak: true})
					{
						return (null, MayflyResult.FromError("NotConnected", "Unable to connect to voice channel and send audio."));
					}
					
					player = await LavaNode.JoinAsync<MayflyPlayer>(Context.Guild.Id, user.VoiceChannel.Id, true);
				}
				else
				{
					return (null, MayflyResult.FromError("NotConnected", "No active bot session."));
				}
			}
			
			if (user.VoiceChannel.Id != player.VoiceChannelId)
			{
				return (null, MayflyResult.FromError("ChannelMismatch", "You are not in the same voice channel."));
			}

			return (player, null);
		}

		private bool IsAdmin()
		{
			return Context.User is SocketGuildUser { GuildPermissions: { Administrator: true } or { ManageGuild: true } };
		}

		private bool IsDJ()
		{
			return Context.User is SocketGuildUser user && user.Roles.Any(r => r.Name.ToLower() == "dj");
		}

		private bool IsRequester(LavalinkTrack track)
		{
			return track?.Context is QueueInfo info && info.User.Id == Context.User.Id;
		}

		[SlashCommand("play", "Play music in discord.")]
		public async Task<RuntimeResult> Play(string query)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer(true);

			if (result is not null)
			{
				return result;
			}

			await DeferAsync();

			LavalinkTrack track = null;

			if (query.Contains("youtube.com") || query.Contains("youtu.be") || query.Contains("soundcloud.com"))
			{
				track = await LavaNode.GetTrackAsync(query);
			}

			if (track is null)
			{
				track = await LavaNode.GetTrackAsync(query, SearchMode.YouTube);

				if (track is null)
				{
					track = await LavaNode.GetTrackAsync(query, SearchMode.SoundCloud);
					
					if (track is null)
					{
						return MayflyResult.FromError("NoResult", "No media was found with that search query.");
					}
				}
			}
			
			QueueInfo info = new QueueInfo(Context.User, Context.Channel);
			track.Context = info;
			
			if (await player.PlayAsync(track, true) > 0)
			{
				await FollowupAsync(embed: await track.GetEmbedAsync("Queued"));
			}
			else
			{
				info.Interaction = Context.Interaction;
			}

			return MayflyResult.FromSuccess();
		}

		[SlashCommand("playlist", "Queue an entire playlist.")]
		public async Task<RuntimeResult> Playlist(string playlist)
		{
			if (Context.User is not IGuildUser user || user.VoiceChannel is null)
			{
				return MayflyResult.FromError("NoVoiceChannel", "User is not in a voice channel.");
			}
			
			List<LavalinkTrack> tracks = (await LavaNode.GetTracksAsync(playlist)).ToList();

			if (!tracks.Any())
			{
				return MayflyResult.FromUserError("EmptyPlaylist", "Playlist is empty, private, or non existent.");
			}
			
			(MayflyPlayer player, MayflyResult result) = await GetPlayer(true);

			if (result is not null)
			{
				return result;
			}
			
			await DeferAsync();

			foreach (LavalinkTrack track in tracks)
			{
				track.Context = new QueueInfo(Context.User, Context.Channel);
				player.Queue.Add(track);
				
				Console.WriteLine(track.Title);
			}
			
			await FollowupAsync("Added all media in playlist to queue.");
			
			if (player.State != PlayerState.Playing)
			{
				LavalinkTrack track = player.Queue.Dequeue();
				await player.PlayTopAsync(track);
			}

			return MayflyResult.FromSuccess();
		}
		
		[SlashCommand("moe", "Queue listen.moe.")]
		public async Task<RuntimeResult> Moe()
		{
			await DeferAsync();
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			LavalinkTrack track = await LavaNode.GetTrackAsync("https://listen.moe/opus");

			if (track is null)
			{
				return MayflyResult.FromError("NoTrack", "Failed to load listen.moe, sorry.");
			}

			QueueInfo info = new QueueInfo(Context.User, Context.Channel);
			track.Context = info;

			if (await player.PlayAsync(track, true) > 0)
			{
				await FollowupAsync(embed: await track.GetEmbedAsync("Queued"));
			}
			else
			{
				info.Interaction = Context.Interaction;
			}

			return MayflyResult.FromSuccess();
		}

		[SlashCommand("skip", "Vote to skip the next song.")]
		public async Task<RuntimeResult> Skip()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (IsAdmin() || IsDJ() || IsRequester(player.CurrentTrack))
			{
				await player.SkipAsync();
				await RespondAsync($"{Context.User.Mention} has skipped the song.");
			}
			else
			{
				UserVoteSkipInfo info = await player.VoteAsync(Context.User.Id);

				if (info.WasAdded)
				{
					await RespondAsync($"{Context.User.Mention} has voted to skip the current track. ({info.Percentage:P})");
				}
			}
			
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("seek", "Seek in track position.")]
		public async Task<RuntimeResult> Seek(TimeSpan time)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (!IsAdmin() && !IsDJ() && !IsRequester(player.CurrentTrack))
			{
				return MayflyResult.FromError("NotPermissible", "You can't seek this track.");
			}
			
			if (player.State == PlayerState.Playing && player.CurrentTrack is {IsSeekable: true})
			{
				if (time > player.CurrentTrack.Duration)
				{
					await player.SkipAsync();
					await RespondAsync("Seeking beyond track length. Skipping.");
				}
				else
				{
					await player.SeekPositionAsync(time);
					await RespondAsync($"Seeking to: `{time}`");
				}
				
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromError("NotSeekable", "This track can't be seeked.");
		}

		[SlashCommand("pause", "Pause/Unpause the track.")]
		public async Task<RuntimeResult> Pause()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ() && !IsRequester(player.CurrentTrack))
			{
				return MayflyResult.FromError("NotPermissible", "You can't pause this track.");
			}

			if (player.State == PlayerState.Paused)
			{
				await player.ResumeAsync();
				await RespondAsync("Resumed track.");
			}
			else
			{
				await player.PauseAsync();
				await RespondAsync("Paused track.");
			}
			
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("stop", "Stop playing and leave.")]
		public async Task<RuntimeResult> Stop()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (IsAdmin() || IsDJ() || IsRequester(player.CurrentTrack) && player.Queue.IsEmpty)
			{
				await player.StopAsync();
				await player.DisconnectAsync();
				await RespondAsync("Cya.");
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("NotPermissible", "You can't stop this player.");
		}

		[SlashCommand("queue", "List track queue.")]
		public async Task<RuntimeResult> Queue()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (player.Queue.IsEmpty)
			{
				await RespondAsync(embed: new EmbedBuilder()
				{
					Title = "Queue",
					Description = "_... empty ..._",
					Color = Color.Red
				}.Build());
			}
			else
			{
				List<EmbedBuilder> builders = player.GetQueuePaged(10).Select(str => new EmbedBuilder().WithDescription(Format.Code(str, "cs"))).ToList();

				await Pagination.SendMessageAsync(Context, new PaginatedMessage(builders, "Media queue", Color.DarkGreen, Context.User, new AppearanceOptions()
				{
					Timeout = TimeSpan.FromMinutes(5),
					Style = DisplayStyle.Full
				}));
			}
			
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("clear", "Clear track queue.")]
		public async Task<RuntimeResult> ClearQueue()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't clear the queue, only admins and DJs can.");
			}
			
			player.Queue.Clear();
			await RespondAsync("Cleared the queue.");
			
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("remove", "Remove track from queue.")]
		public async Task<RuntimeResult> Remove(int index)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (--index > player.Queue.Count - 1 || 0 > index)
			{
				return MayflyResult.FromUserError("InvalidIndex", "Index is out of queue range.");
			}

			LavalinkTrack track = player.Queue[index];

			if (!IsAdmin() && !IsDJ() && !IsRequester(track))
			{
				return MayflyResult.FromError("NotPermissible", "You can't remove that track from the queue.");
			}

			await RespondAsync(embed: await track.GetEmbedAsync("Removed"));
			player.Queue.RemoveAt(index);
			
			return MayflyResult.FromSuccess();
		}
		
		[SlashCommand("shuffle", "Shuffle track queue.")]
		public async Task<RuntimeResult> Shuffle()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't shuffle the queue, only admins and DJs can.");
			}
			
			player.Queue.Shuffle();
			await RespondAsync("Shuffled.");
			
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("random", "Play a random track in the queue.")]
		public async Task<RuntimeResult> Random()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (player.Queue.IsEmpty)
			{
				return MayflyResult.FromError("QueueEmpty", "The queue has no tracks.");
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't pick a random track from the queue, only admins and DJs can.");
			}
			
			LavalinkTrack track = player.Queue[RandomService.Next(0, player.Queue.Count - 1)];

			await player.PlayTopAsync(track);
			await RespondAsync(embed: await track.GetEmbedAsync());

			return MayflyResult.FromSuccess();
		}

		[SlashCommand("track", "Currently playing track.")]
		public async Task<RuntimeResult> Track()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (player.State == PlayerState.NotPlaying)
			{
				return MayflyResult.FromError("NoTrack", "Nothing is currently playing.");
			}

			await RespondAsync(embed: await player.CurrentTrack.GetEmbedAsync());
			return MayflyResult.FromSuccess();
		}
		
		[SlashCommand("reset", "Reset all audio effects.")]
		public async Task<RuntimeResult> Reset()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}

			player.Filters.Distortion = null;
			player.Filters.Equalizer = null;
			player.Filters.Karaoke = null;
			player.Filters.Rotation = null;
			player.Filters.Timescale = null;
			player.Filters.Tremolo = null;
			player.Filters.Vibrato = null;
			player.Filters.Volume = null;
			player.Filters.ChannelMix = null;
			player.Filters.LowPass = null;

			await player.Filters.CommitAsync();
			await RespondAsync("Reset filters.");
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("boost", "Boost the lower band range.")]
		public async Task<RuntimeResult> Boost([MinValue(-0.25f), MaxValue(1.0f)] float amount = 1)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}

			player.Filters.Equalizer = new EqualizerFilterOptions()
			{
				Bands = Enumerable.Range(0, 3).Select(x => new EqualizerBand(x, amount)).ToArray()
			};

			await player.Filters.CommitAsync(true);
			await RespondAsync("Boosting lower band.");
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("bend", "Randomize the audio bands.")]
		public async Task<RuntimeResult> Bend()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}

			player.Filters.Equalizer = new EqualizerFilterOptions()
			{
				Bands = Enumerable.Range(0, 14).Select(x => new EqualizerBand(x, RandomService.NextFloat(-0.25f, 1.0f))).ToArray()
			};

			await player.Filters.CommitAsync();
			await RespondAsync("Randomizing the audio bands.");
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("vibrato", "Wavy funny.")]
		public async Task<RuntimeResult> Vibrato([MinValue(5f), MaxValue(15f)] float strength = 5)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}

			player.Filters.Vibrato = new VibratoFilterOptions()
			{
				Depth = 1f,
				Frequency = strength
			};

			await player.Filters.CommitAsync();
			await RespondAsync("Wavy time.");
			return MayflyResult.FromSuccess();
		}
		
		[SlashCommand("nuke", "Random distortion™")]
		public async Task<RuntimeResult> Nuke()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}

			player.Filters.Distortion = new DistortionFilterOptions()
			{
				SinOffset = RandomService.Next(0, 10),
				SinScale = RandomService.Next(0, 10),
				CosOffset = RandomService.Next(0, 10),
				CosScale = RandomService.Next(0, 10),
				TanOffset = RandomService.Next(0, 10),
				TanScale = RandomService.Next(0, 10),
				Offset = RandomService.Next(0, 10),
				Scale = RandomService.Next(0, 10)
			};
			
			await player.Filters.CommitAsync();
			await RespondAsync("Applying Random distortion™");
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("rape", "CBT for your ears.")]
		public async Task<RuntimeResult> Rape([MinValue(2f), MaxValue(10f)] float pain = 3)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}
			
			player.Filters.Equalizer = new EqualizerFilterOptions()
			{
				Bands = new EqualizerBand[14]
				{
					new (0, 1), new (1, 1), new (2, 1), new (3, 1),
					new (4, 0), new (5, 0), new (6, 0), new (7, 0),
					new (8, 0), new (9, 0), new (10, 0), new (11, 0),
					new (12, 0), new (13, 0),
				}
			};

			player.Filters.Volume = new VolumeFilterOptions()
			{
				Volume = pain
			};
			
			await player.Filters.CommitAsync();
			await RespondAsync("I'm sorry.");
			return MayflyResult.FromSuccess();
		}

		[SlashCommand("vaporwave", "Crackhead energy.")]
		public async Task<RuntimeResult> Vaporwave()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}

			player.Filters.Timescale = new TimescaleFilterOptions()
			{
				Speed = 1,
				Pitch = 1,
				Rate = 0.7f
			};

			await player.Filters.CommitAsync();
			await RespondAsync("You probably drink lean, don't you.");
			return MayflyResult.FromSuccess();
		}
		
		[SlashCommand("nightcore", "Relive 2010 YouTube.")]
		public async Task<RuntimeResult> Nightcore()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (!IsAdmin() && !IsDJ())
			{
				return MayflyResult.FromError("NotPermissible", "You can't modify filters, only admins and DJs can.");
			}
			
			player.Filters.Timescale = new TimescaleFilterOptions()
			{
				Speed = 1,
				Pitch = 1,
				Rate = 1.3f
			};
			
			await player.Filters.CommitAsync();
			await RespondAsync("Time traveling...");
			return MayflyResult.FromSuccess();
		}
	}
}