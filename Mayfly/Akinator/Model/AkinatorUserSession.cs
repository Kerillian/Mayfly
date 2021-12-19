namespace Mayfly.Akinator.Model
{
	public class AkinatorUserSession
	{
		public string Session { get; }
		public string Signature { get; }
		public int Step { get; }
		public int LastGuessStep { get; }
		
		public AkinatorUserSession(string session, string signature, int step, int lastGuessStep)
		{
			Session = session;
			Signature = signature;
			Step = step;
			LastGuessStep = lastGuessStep;
		}
	}
}