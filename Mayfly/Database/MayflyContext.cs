using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Mayfly.Database
{
	public class MayflyContext : DbContext
	{
		public virtual DbSet<UserData> Users { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			options.UseSqlite(new SqliteConnection(new SqliteConnectionStringBuilder()
			{
				DataSource = "sqlite.db"
			}.ToString()));
		}
	}
}