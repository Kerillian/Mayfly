using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Mayfly.Attributes;
using Mayfly.Services;

namespace Mayfly.Modules
{
	/*
	 * Not fully tested and might be broken, so Hidden it is.
	 */
	[RequireOwner, Group("settings"), Hidden]
	public class SettingsModule : MayflyModule
	{
		public DatabaseService Database { get; set; }

		[Command("announcement"), Summary("Set the announcement channel.")]
		public async Task AnnouncementChannel()
		{
			await Database.ModifyGuildAsync(Context.Guild, data =>
			{
				data.AnnouncementId = Context.Channel.Id;
			});
		}
		
		[Command("mute"), Summary("Mute announcements.")]
		public async Task Mute()
		{
			bool muted = false;
			
			await Database.ModifyGuildAsync(Context.Guild, data =>
			{
				data.BlockAnnouncement = !data.BlockAnnouncement;
				muted = data.BlockAnnouncement;
			});

			await ReplyToAsync($"Announcements {(muted ? "muted" : "unmuted")}");
		}
	}
}