using Mayfly.Akinator.Enumerations;

namespace Mayfly.Akinator.Model
{
	public class AnswerRequest
	{
		public AnswerOptions Choice { get; }
		public int Step { get; }
		public string Session { get; }
		public string Signature { get; }

		public AnswerRequest(AnswerOptions choice, int step, string session, string signature)
		{
			Choice = choice;
			Step = step;
			Session = session;
			Signature = signature;
		}
	}
}