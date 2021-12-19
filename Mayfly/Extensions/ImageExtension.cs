using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Mayfly.Extensions
{
	public static class ImageExtension
	{
		public static string SaveAsGifOrPng(this Image image, Stream stream, GifEncoder gifEncoder = null, PngEncoder pngEncoder = null)
		{
			if (image.Frames.Any() && image.Frames.Count > 1)
			{
				image.SaveAsGif(stream, gifEncoder);
				return ".gif";
			}
			
			image.SaveAsPng(stream, pngEncoder);
			return ".png";
		}
		
		public static async Task<string> SaveAsGifOrPngAsync(this Image image, Stream stream, GifEncoder gifEncoder = null, PngEncoder pngEncoder = null)
		{
			if (image.Frames.Any() && image.Frames.Count > 1)
			{
				await image.SaveAsGifAsync(stream, gifEncoder);
				return ".gif";
			}
			
			await image.SaveAsPngAsync(stream, pngEncoder);
			return ".png";
		}
		
		public static string SaveAsGifOrJpeg(this Image image, Stream stream, GifEncoder gifEncoder = null, JpegEncoder jpegEncoder = null)
		{
			if (image.Frames.Any() && image.Frames.Count > 1)
			{
				image.SaveAsGif(stream, gifEncoder);
				return ".gif";
			}
			
			image.SaveAsJpeg(stream, jpegEncoder);
			return ".jpg";
		}
		
		public static async Task<string> SaveAsGifOrJpegAsync(this Image image, Stream stream, GifEncoder gifEncoder = null, JpegEncoder jpegEncoder = null)
		{
			if (image.Frames.Any() && image.Frames.Count > 1)
			{
				await image.SaveAsGifAsync(stream, gifEncoder);
				return ".gif";
			}
			
			await image.SaveAsJpegAsync(stream, jpegEncoder);
			return ".jpg";
		}
	}
}