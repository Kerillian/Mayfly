namespace Mayfly.Akinator.Model
{
	public class AkinatorQuestion
	{
		public string Text { get; }
		public double Progression { get; }
		public int Step { get; }

		public AkinatorQuestion(string text, double progression, int step)
		{
			Text = text;
			Progression = progression;
			Step = step;
		}
	}
}