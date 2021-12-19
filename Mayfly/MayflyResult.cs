using Discord.Commands;

namespace Mayfly
{
	public class MayflyResult : RuntimeResult
	{
		public string Message { get; }

		public MayflyResult(CommandError? error, string reason, string message) : base(error, reason)
		{
			this.Message = message;
		}

		public static MayflyResult FromError(string reason, string message)
		{
			return new MayflyResult(CommandError.Exception, reason, message);
		}
		
		public static MayflyResult FromUserError(string reason, string message)
		{
			return new MayflyResult(CommandError.ParseFailed, reason, message);
		}

		public static MayflyResult FromSuccess(string reason = null, string message = null)
		{
			return new MayflyResult(null, reason, message);
		}
	}
}