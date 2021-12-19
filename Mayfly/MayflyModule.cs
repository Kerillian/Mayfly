using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Mayfly
{
	public sealed class InteractivityResult<T>
	{
		public T Value { get; }
		public TimeSpan Elapsed { get; }
		public bool IsTimedout { get; }
		public bool IsCancelled { get; }
		public bool IsSuccess => !IsCancelled && !IsTimedout;

		internal InteractivityResult(T value, TimeSpan elapsed, bool isTimedout, bool isCancelled)
		{
			Value = value;
			Elapsed = elapsed;
			IsTimedout = isTimedout;
			IsCancelled = isCancelled;
		}
	}
	
	[RequireContext(ContextType.Guild)]
	public class MayflyModule : ModuleBase<SocketCommandContext>
	{
		private static readonly Regex CodeBlockPattern = new Regex(@"```(.*)\n([\s\S]*)?\n```", RegexOptions.Compiled);
		private CancellationTokenSource source = new CancellationTokenSource();
		private TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

		protected async Task Log(params object[] args)
		{
			await Console.Out.WriteLineAsync(string.Join(", ", args));
		}

		protected bool TryGetAttachmentUrl(out string url)
		{
			foreach (Attachment attachment in Context.Message.Attachments)
			{
				if (attachment.Width.HasValue && attachment.Height.HasValue)
				{
					url = attachment.Url;
					return true;
				}
			}
			
			url = null;
			return false;
		}

		protected bool TryGetCode(string body, out string lang, out string code)
		{
			Match match = CodeBlockPattern.Match(body);

			if (match.Groups.Count == 3)
			{
				lang = match.Groups[1].Value;
				code = match.Groups[2].Value;
				return true;
			}

			lang = null;
			code = null;
			return false;
		}

		protected bool TryGetCode(out string lang, out string code)
		{
			return this.TryGetCode(this.Context.Message.Content, out lang, out code);
		}

		// protected async Task<IUserMessage> ReplyErrorAsync(string error, string message, RequestOptions options = null)
		// {
		// 	return await this.Context.Channel.SendMessageAsync("", false, new EmbedBuilder()
		// 	{
		// 		Title = "Error: " + error,
		// 		Description = message,
		// 		Color = Color.Red
		// 	}.Build(), options);
		// }

		protected async Task<IUserMessage> ReplySanitizedAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent component = null, ISticker[] stickers = null, Embed[] embeds = null)
		{
			return await this.Context.Channel.SendMessageAsync(Format.Sanitize(text), isTTS, embed, options, allowedMentions, messageReference, component, stickers, embeds);
		}

		protected async Task<IUserMessage> ReplyFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent component = null, ISticker[] stickers = null, Embed[] embeds = null)
		{
			return await this.Context.Channel.SendFileAsync(filePath, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference, component, stickers, embeds);
		}

		protected async Task<IUserMessage> ReplyFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent component = null, ISticker[] stickers = null, Embed[] embeds = null)
		{
			return await this.Context.Channel.SendFileAsync(stream, filename, text, isTTS, embed, options, isSpoiler, allowedMentions, messageReference, component, stickers, embeds);
		}

		protected async Task<IUserMessage> ReplyToAsync(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageComponent component = null, ISticker[] stickers = null, Embed[] embeds = null)
		{
			//return await this.Context.Channel.SendMessageAsync(message == null ? this.Context.User.Mention : this.Context.User.Mention + " " + message, isTTS, embed, options);
			return await this.Context.Channel.SendMessageAsync(message, isTTS, embed, options, allowedMentions, new MessageReference(Context.Message.Id, Context.Channel.Id, Context.Guild.Id), component, stickers, embeds);
		}

		protected async Task<IUserMessage> ReplyCodeAsync(string code, string lang = "", bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent component = null, ISticker[] stickers = null, Embed[] embeds = null)
		{
			return await this.Context.Channel.SendMessageAsync(Format.Code(code, lang), isTTS, embed, options, allowedMentions, messageReference, component, stickers, embeds);
		}

		public async Task<InteractivityResult<SocketMessageComponent>> NextInteractionAsync(Predicate<SocketMessageComponent> filter = null,
			Func<SocketMessageComponent, bool, Task> actions = null, TimeSpan? timeout = null, bool? runOnGateway = null,
			CancellationToken cancellationToken = default)
		{
			DateTime startTime = DateTime.UtcNow;
			
			filter ??= _ => true;
			actions ??= (_, _) => Task.CompletedTask;
			
			TaskCompletionSource<InteractivityResult<SocketMessageComponent>> reactionSource = new TaskCompletionSource<InteractivityResult<SocketMessageComponent>>();
			TaskCompletionSource<bool> cancelSource = new TaskCompletionSource<bool>();
			
			CancellationTokenRegistration cancellationRegistration = cancellationToken.Register(() => cancelSource.SetResult(true));

			Task<InteractivityResult<SocketMessageComponent>> reactionTask = reactionSource.Task;
			Task<bool> cancelTask = cancelSource.Task;
			Task timeoutTask = Task.Delay(timeout ?? TimeSpan.FromSeconds(20));

			async Task CheckComponentAsync(SocketMessageComponent component)
			{
				if (component.User.Id == Context.Client.CurrentUser.Id)
				{
					return;
				}
				if (!filter.Invoke(component))
				{
					await actions.Invoke(component, true).ConfigureAwait(false);
					return;
				}

				await actions.Invoke(component, false).ConfigureAwait(false);
				reactionSource.SetResult(new InteractivityResult<SocketMessageComponent>(component, DateTime.UtcNow - startTime, false, false));
			}
			
			async Task HandleReactionAsync(SocketMessageComponent component)
			{
				if (runOnGateway ?? false)
				{
					await CheckComponentAsync(component);
				}
				else
				{
					_ = Task.Run(() => CheckComponentAsync(component));
				}
			}
			
			try
			{
				Context.Client.ButtonExecuted += HandleReactionAsync;

				Task result = await Task.WhenAny(reactionTask, cancelTask, timeoutTask).ConfigureAwait(false);

				return result == reactionTask
					? await reactionTask.ConfigureAwait(false)
					: result == timeoutTask
						? new InteractivityResult<SocketMessageComponent>(default, timeout ?? DefaultTimeout, true, false)
						: new InteractivityResult<SocketMessageComponent>(default, DateTime.UtcNow - startTime, false, true);
			}
			finally
			{
				Context.Client.ButtonExecuted -= HandleReactionAsync;
				cancellationRegistration.Dispose();
			}
		}
	}
}