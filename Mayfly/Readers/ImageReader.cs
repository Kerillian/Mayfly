using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Services;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Mayfly.Readers
{
	public class ImageReader : TypeReader
	{
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (services.GetService(typeof(HttpService)) is not HttpService http)
			{
				return TypeReaderResult.FromError(CommandError.Exception, "HttpService not found.");
			}

			Image image;
			
			foreach (IAttachment attachment in context.Message.Attachments)
			{
				if (attachment.Width.HasValue && attachment.Height.HasValue)
				{
					image = await http.GetMediaAsync(attachment.Url);

					if (image is not null)
					{
						return TypeReaderResult.FromSuccess(image);
					}
					
					break;
				}
			}

			image = await http.GetMediaAsync(input);
			return await http.GetMediaAsync(input) is not null ? TypeReaderResult.FromSuccess(image) : TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse image.");
		}
	}
}