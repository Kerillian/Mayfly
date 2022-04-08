namespace Mayfly.Services.Trivia
{
	public class TriviaPlayer
	{
		public int Score { get; set; }
		public int Correct { get; set; }
		public int Wrong { get; set; }
		public int Streak { get; set; }
		public int BestStreak { get; set; }
		public bool HasChosen { get; set; }
		public string Username { get; }

		public TriviaPlayer(string username)
		{
			this.Username = username;
		}
	}
}