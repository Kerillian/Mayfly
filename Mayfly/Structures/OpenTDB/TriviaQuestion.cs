using Newtonsoft.Json;

namespace Mayfly.Structures
{
	public class TriviaQuestion
	{
		[JsonProperty("category")]
		public string Category { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("difficulty")]
		public string Difficulty { get; set; }

		[JsonProperty("question")]
		public string Question { get; set; }

		[JsonProperty("correct_answer")]
		public string CorrectAnswer { get; set; }

		[JsonProperty("incorrect_answers")]
		public string[] IncorrectAnswers { get; set; }

		[JsonIgnore]
		public bool IsBoolean => Type == "boolean";

		[JsonIgnore]
		public string[] ShuffledItems => IncorrectAnswers.Append(CorrectAnswer).OrderBy(_ => Guid.NewGuid()).ToArray();

		public bool IsCorrect(string answer)
		{
			return answer == CorrectAnswer;
		}
	}
}