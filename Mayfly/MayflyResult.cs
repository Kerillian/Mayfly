using Discord.Commands;
using Discord.Interactions;
using RuntimeResult = Discord.Commands.RuntimeResult;

namespace Mayfly
{

	public class MayflyResult : Discord.Interactions.RuntimeResult
	{
		public string Message { get; }

		public MayflyResult(InteractionCommandError? error, string reason, string message) : base(error, reason)
		{
			this.Message = message;
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