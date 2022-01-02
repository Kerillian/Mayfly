using System.ComponentModel.DataAnnotations;

namespace Mayfly.Database
{
	public class GuildData
	{
		[Key]
		public ulong GuildId { get; set; }

		public ulong AnnouncementId { get; set; }
		public bool BlockAnnouncement { get; set; }
	}
}