using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mayfly.Database
{
	public class ItemData
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public uint Index { get; set; }
		
		public ulong UserId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string ImgurId { get; set; }
		public uint Color { get; set; }
		public int Amount { get; set; }
		public uint Cost { get; set; }
	}
}