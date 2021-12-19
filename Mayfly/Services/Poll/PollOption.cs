namespace Mayfly.Services.Poll
{
	public class PollOption
	{
		public string Name { get; }
		public int Votes { get; set; }

		public PollOption(string name)
		{
			this.Name = name;
			this.Votes = 0;
		}

		public string Percentage(int total)
		{
			return total == 0 ? "0.00%" : $"{(float)this.Votes / total:0.00%}";
		}
		
		public string Bar(int total)
		{
			int chunks = (int)(50f / total) * this.Votes;
			int left = 50 - chunks;
			
			return "`" + new string('█', chunks) + new string(' ', left) + "`";
		}
	}
}