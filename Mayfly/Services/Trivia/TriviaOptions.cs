using System.Text;

namespace Mayfly.Services.Trivia
{
	public enum TriviaDifficulty
	{
		All, Easy, Medium, Hard
	}

	public enum TriviaCategory
	{
		All, GeneralKnowledge = 9, Books, Film, Music, MusicalsAndTheatres, Television, VideoGames, BoardGames,
		ScienceAndNature, Computers, Mathematics, Mythology, Sports, Geography, History, Politics, Art, Celebrities,
		Animals, Vehicles, Comics, Gadgets, AnimeAndManga, CartoonAnimations
	}

	public enum TriviaType
	{
		All, Multiple, Boolean
	}

	public class TriviaOptions
	{
		public int Total { get; set; }
		public TriviaCategory Category { get; set; }
		public TriviaDifficulty Difficulty { get; set; }
		public TriviaType Type { get; set; }

		public TriviaOptions(int total, TriviaCategory category, TriviaDifficulty difficulty, TriviaType type)
		{
			Total = total;
			Category = category;
			Difficulty = difficulty;
			Type = type;
		}

		public string Build()
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("https://opentdb.com/api.php?amount=" + Total);

			if (Category > 0)
			{
				builder.Append("&category=" + (int)Category);
			}

			if (Difficulty > 0)
			{
				builder.Append("&difficulty=" + Enum.GetName(typeof(TriviaDifficulty), Difficulty)?.ToLower());
			}

			if (Type > 0)
			{
				builder.Append("&type=" + Enum.GetName(typeof(TriviaType), Type)?.ToLower());
			}

			return builder.ToString();
		}
	}
}