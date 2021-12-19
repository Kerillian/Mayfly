using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mayfly.Database
{
	public class UserData
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong UserId { get; set; }
		public ulong Experience { get; set; }
		public ulong Invokes { get; set; }
		
		public long Money { get; set; }
		public long Tokens { get; set; }
		public long InvestedMoney { get; set; }
		public long InvestedTokens { get; set; }
		
		public DateTime LastDrop { get; set; }
	}
}