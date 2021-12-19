using System;
using System.Linq;
using System.Threading.Tasks;
using Mayfly.Extensions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Filters;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Mayfly.Attributes.Parameter;
using Mayfly.Services;
using Mayfly.Utilities;

namespace Mayfly.Modules
{
	public class RequestInfo
	{
		public IUser User { get; }
		public ISocketMessageChannel Channel { get; }

		public RequestInfo(IUser user, ISocketMessageChannel channel)
		{
			this.User = user;
			this.Channel = channel;
		}
	}
	
	[RequireContext(ContextType.Guild), Group("music"), Alias("m"), Summary("Music Module")]
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

			if (player is null)
			{
				if (autoConnect)
				{
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
		
		[Command("play"), Summary("Play music in discord.")]
		public async Task<RuntimeResult> Play([Remainder] string query)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer(true);

			if (result is not null)
			{
				return result;
			}

			LavalinkTrack track = await LavaNode.GetTrackAsync(query, SearchMode.YouTube);

			if (track is null)
			{
				track = await LavaNode.GetTrackAsync(query, SearchMode.SoundCloud);

				if (track is null)
				{
					return MayflyResult.FromError("NoResult", "No media was found with that search query.");
				}
			}
			
			track.Context = new RequestInfo(Context.User, Context.Channel);

			int position = await player.PlayAsync(track, true);
			await ReplyAsync(embed: await track.GetEmbedAsync(position == 0 ? "Playing" : "Queued"));

			return MayflyResult.FromSuccess();
		}

		[Command("skip"), Summary("Vote to skip the next song.")]
		public async Task<RuntimeResult> Skip()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (player.CurrentTrack?.Context is RequestInfo request && request.User.Id == Context.User.Id)
			{
				await player.SkipAsync();
			}
			else
			{
				UserVoteSkipInfo info = await player.VoteAsync(Context.User.Id);

				if (info.WasAdded)
				{
					await ReplyAsync($"{Context.User.Mention} has voted to skip the current track. ({info.Percentage:P})");
				}
			}
			
			return MayflyResult.FromSuccess();
		}

		[Command("seek"), Summary("Seek in track position.")]
		public async Task<RuntimeResult> Seek(TimeSpan time)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (player.State == PlayerState.Playing && player.CurrentTrack is {IsSeekable: true})
			{
				if (time > player.CurrentTrack.Duration)
				{
					await player.SkipAsync();
				}
				else
				{
					await player.SeekPositionAsync(time);
				}
				
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromError("NotSeekable", "This track can't be seeked.");
		}

		[Command("pause"), Alias("resume", "unpause"), Summary("Pause/Unpause the track.")]
		public async Task<RuntimeResult> Pause()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (player.State == PlayerState.Paused)
			{
				await player.ResumeAsync();
			}
			else
			{
				await player.ResumeAsync();
			}
			
			return MayflyResult.FromSuccess();
		}

		[Command("stop"), Alias("cya", "leave", "die"), Summary("Stop playing and leave.")]
		public async Task<RuntimeResult> Stop()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			await player.StopAsync();
			await player.DisconnectAsync();
			
			return MayflyResult.FromSuccess();
		}

		[Command("queue"), Alias("q"), Summary("List track queue.")]
		public async Task<RuntimeResult> Queue()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			if (player.Queue.IsEmpty)
			{
				await ReplyAsync(embed: new EmbedBuilder()
				{
					Title = "Queue",
					Description = "_... empty ..._",
					Color = Color.Red
				}.Build());
			}
			else
			{
				await Pagination.SendMessageAsync(Context.Channel, new PaginatedMessage(player.GetQueuePaged(20).Select(x => new EmbedBuilder().WithDescription($"```cs\n{x}\n```")), null, Color.DarkGreen, Context.User, new AppearanceOptions()
				{
					Timeout = TimeSpan.FromMinutes(5),
					Style = DisplayStyle.Full
				}));
			}
			
			return MayflyResult.FromSuccess();
		}

		[Command("clear"), Summary("Clear track queue.")]
		public async Task<RuntimeResult> ClearQueue()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			player.Queue.Clear();
			await ReplyAsync("Cleared the queue.");
			
			return MayflyResult.FromSuccess();
		}

		[Command("remove"), Summary("Remove track from queue.")]
		public async Task<RuntimeResult> Remove(int index)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			if (index > player.Queue.Count - 1 || 0 > index)
			{
				return MayflyResult.FromUserError("InvalidIndex", "Index is out of queue range.");
			}

			LavalinkTrack track = player.Queue[index];

			await ReplyAsync(embed: await track.GetEmbedAsync("Removed"));
			player.Queue.RemoveAt(index);
			
			return MayflyResult.FromSuccess();
		}
		
		[Command("shuffle"), Summary("Shuffle track queue.")]
		public async Task<RuntimeResult> Shuffle()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			player.Queue.Shuffle();
			await ReplyToAsync("Shuffled.");
			
			return MayflyResult.FromSuccess();
		}

		[Command("random"), Summary("Play a random track in the queue.")]
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
			
			LavalinkTrack track = player.Queue[RandomService.Next(0, player.Queue.Count - 1)];

			await player.PlayTopAsync(track);
			await ReplyAsync(embed: await track.GetEmbedAsync("Playing"));
			
			return MayflyResult.FromSuccess();
		}

		[Command("track"), Alias("np"), Summary("Currently playing track.")]
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

			await ReplyAsync(embed: await player.CurrentTrack.GetEmbedAsync());
			return MayflyResult.FromSuccess();
		}
		
		[Command("reset"), Summary("Reset all audio effects.")]
		public async Task<RuntimeResult> Reset()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
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
			return MayflyResult.FromSuccess();
		}

		[Command("boost"), Summary("Boost the lower band range.")]
		public async Task<RuntimeResult> Boost([Range(-0.25, 1.0)] float amount = 1)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			player.Filters.Equalizer = new EqualizerFilterOptions()
			{
				Bands = Enumerable.Range(0, 2).Select(x => new EqualizerBand(x, amount)).ToArray()
			};
			
			return MayflyResult.FromSuccess();
		}

		[Command("bend"), Summary("Randomize the audio bands.")]
		public async Task<RuntimeResult> Bend()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			player.Filters.Equalizer = new EqualizerFilterOptions()
			{
				Bands = Enumerable.Range(0, 14).Select(x => new EqualizerBand(x, RandomService.NextFloat(-0.25f, 1.0f))).ToArray()
			};

			await player.Filters.CommitAsync();
			return MayflyResult.FromSuccess();
		}

		[Command("vibrato"), Summary("Reset all audio bands.")]
		public async Task<RuntimeResult> Vibrato([Range(5.0f, 14.0f)] float strength = 5)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			player.Filters.Vibrato = new VibratoFilterOptions()
			{
				Depth = 1f,
				Frequency = strength
			};

			await player.Filters.CommitAsync();
			return MayflyResult.FromSuccess();
		}
		
		[Command("nuke"), Summary("Random distortion™")]
		public async Task<RuntimeResult> Nuke()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
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
			return MayflyResult.FromSuccess();
		}

		[Command("rape"), Summary("CBT for your ears.")]
		public async Task<RuntimeResult> Rape([Range(2f, 10f)] float pain = 3)
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			player.Filters.Equalizer = new EqualizerFilterOptions()
			{
				Bands = new EqualizerBand[14]
				{
					new EqualizerBand(0, 1),
					new EqualizerBand(1, 1),
					new EqualizerBand(2, 1),
					new EqualizerBand(3, 1),
					new EqualizerBand(4, 0.5f),
					new EqualizerBand(5, 0.5f),
					new EqualizerBand(6, 0.5f),
					new EqualizerBand(7, 0.5f),
					new EqualizerBand(8, 0.25f),
					new EqualizerBand(9, 0.25f),
					new EqualizerBand(10, 0.25f),
					new EqualizerBand(11, 0.25f),
					new EqualizerBand(12, 0.25f),
					new EqualizerBand(13, 0.25f),
				}
			};

			player.Filters.Volume = new VolumeFilterOptions()
			{
				Volume = pain
			};
			
			await player.Filters.CommitAsync();
			return MayflyResult.FromSuccess();
		}

		[Command("vaporwave"), Summary("Crackhead energy.")]
		public async Task<RuntimeResult> Vaporwave()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}

			player.Filters.Timescale = new TimescaleFilterOptions()
			{
				Speed = 1,
				Pitch = 1,
				Rate = 0.7f
			};

			await player.Filters.CommitAsync();
			return MayflyResult.FromSuccess();
		}
		
		[Command("nightcore"), Summary("Relive 2010 YouTube.")]
		public async Task<RuntimeResult> Nightcore()
		{
			(MayflyPlayer player, MayflyResult result) = await GetPlayer();

			if (result is not null)
			{
				return result;
			}
			
			player.Filters.Timescale = new TimescaleFilterOptions()
			{
				Speed = 1,
				Pitch = 1,
				Rate = 1.3f
			};
			
			await player.Filters.CommitAsync();
			return MayflyResult.FromSuccess();
		}
	}
}