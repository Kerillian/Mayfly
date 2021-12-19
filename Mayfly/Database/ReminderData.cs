using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mayfly.Database
{
	public class ReminderData
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public uint Index { get; set; }
		
		public ulong UserId { get; set; }
		public string Message { get; set; }
		public DateTime DueDate { get; set; }
	}
}