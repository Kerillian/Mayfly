namespace Mayfly.Akinator.Model
{
	public class GuessRequest
	{
		public int Step { get; }
		public string Session { get; }
		public string Signature { get; }
		
		public GuessRequest(int step, string session, string signature)
		{
			Step = step;
			Session = session;
			Signature = signature;
		}
	}
}