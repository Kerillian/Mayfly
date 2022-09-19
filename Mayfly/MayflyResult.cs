using Discord.Interactions;

namespace Mayfly
{
	public class MayflyResult : RuntimeResult
	{
		public string Message { get; }

		private MayflyResult(InteractionCommandError? error, string reason, string message) : base(error, reason)
		{
			Message = message;
		}

		public static MayflyResult FromError(string reason, string message)
		{
			return new MayflyResult(InteractionCommandError.Exception, reason, message);
		}
		
		public static MayflyResult FromUserError(string reason, string message)
		{
			return new MayflyResult(InteractionCommandError.ParseFailed, reason, message);
		}

		public static MayflyResult FromSuccess(string reason = null, string message = null)
		{
			return new MayflyResult(null, reason, message);
		}
	}
}