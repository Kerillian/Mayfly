using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mayfly.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Mayfly.Extensions;
using Mayfly.Attributes.Parameter;
using Mayfly.Services;
using Mayfly.Structures;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;

namespace Mayfly.Modules
{
	public class ImageModule : MayflyModule
	{
		public HttpService http { get; set; }
		public RandomService random { get; set; }
		public BotConfig config { get; set; }

		[Command("oil"), Summary("Make anything art."), RateLimit(15)]
		public async Task<RuntimeResult> Oil(string url, [Range(1, 50)] int levels = 25, [Range(1, 50)] int size = 30)
		{
			using Image image = await this.http.GetMediaAsync(url);
			
			if (image != null)
			{
				image.Mutate(x => x.OilPaint(levels, size));

				await using MemoryStream stream = new MemoryStream();
				string ext = await image.SaveAsGifOrPngAsync(stream);
				
				stream.Seek(0, SeekOrigin.Begin);
				await this.ReplyFileAsync(stream, "oil" + ext);
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load image.");
		}

		[Command("oil"), Summary("Make anything art."), RateLimit(15)]
		public async Task<RuntimeResult> Oil([Range(1, 50)] int levels = 25, [Range(1, 50)] int size = 30)
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.Oil(url, levels, size);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}

		[Command("corrupt"), Alias("glitch"), Summary("Corrupt jpeg image data.")]
		public async Task<RuntimeResult> Corrupt(string url, [Range(5, 100)] int Iterations = 25)
		{
			using Image<Rgba32> image = await this.http.GetImageAsync<Rgba32>(url);
			
			if (image != null)
			{
				await using MemoryStream stream = new MemoryStream();
				await image.SaveAsJpegAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);
				byte[] buffer = stream.ToArray();
				int header = 417;

				for (int i = 0, len = buffer.Length; i < len; i++)
				{
					if (buffer[i] == 0xFF && buffer[i + 1] == 0xDA)
					{
						header = (417 + 2) + i;
					}
				}

				for (int i = 0; i < Iterations; i++)
				{
					buffer[this.random.Next(header + 4, buffer.Length)] = 0x00;
				}

				await using MemoryStream corruptedStream = new MemoryStream(buffer);
				await this.ReplyFileAsync(corruptedStream, "corrupted.jpg");
			}
			else
			{
				return MayflyResult.FromUserError("LoadFailed", "Failed to load image.");
			}
			
			return MayflyResult.FromSuccess();
		}
	
		[Command("corrupt"), Alias("glitch"), Summary("Corrupt jpeg image data.")]
		public async Task<RuntimeResult> Corrupt([Range(5, 100)] int iterations = 25)
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.Corrupt(url, iterations);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}
		
		[Command("crush"), Summary("Crushes the image resolution.")]
		public async Task<RuntimeResult> Crush(string url, [Range(0f, 1f)] float scale = 0.5f, [Range(1, 100)] int quality = 10)
		{
			using Image image = await this.http.GetMediaAsync(url);
			
			if (image != null)
			{
				image.Mutate(x => {
					(int w, int h) = x.GetCurrentSize();
					x.Resize((int)(w * scale), (int)(h * scale), KnownResamplers.NearestNeighbor, false);
					x.Resize(w, h, KnownResamplers.NearestNeighbor, false);
				});

				await using MemoryStream stream = new MemoryStream();
				string ext = await image.SaveAsGifOrJpegAsync(stream, jpegEncoder: new JpegEncoder()
				{
					Quality = quality
				});

				stream.Seek(0, SeekOrigin.Begin);
				await this.ReplyFileAsync(stream, "crushed" + ext);
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("LoadFailed", "Failed to load image.");
		}

		[Command("crush"), Summary("Crushes the image resolution.")]
		public async Task<RuntimeResult> Crush()
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.Crush(url);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}

		private void ColorFuckWorker(Image<Rgba32> image)
		{
			for (int y = 0; y < image.Height; y++)
			{
				Span<Rgba32> row = image.GetPixelRowSpan(y);
				
				for (int x = 0; x < image.Width; x++)
				{
					Rgba32 c = row[x];
					row[x] = new Rgba32(c.R >> ~(c.R / 2), c.G >> ~(c.G / 2), c.B >> ~(c.B / 2), c.A);
				}
			}
		}

		[Command("shift"), Summary("Messes with image color channels.")]
		public async Task<RuntimeResult> Shift(string url)
		{
			using Image<Rgba32> image = await this.http.GetImageAsync<Rgba32>(url);
			if (image != null)
			{
				this.ColorFuckWorker(image);

				await using MemoryStream stream = new MemoryStream();
				await image.SaveAsJpegAsync(stream, new JpegEncoder()
				{
					Quality = 5
				});

				stream.Seek(0, SeekOrigin.Begin);
				await this.ReplyFileAsync(stream, "shifted.jpg");
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("InvalidUrl", "Url provided by user is invalid.");
		}

		[Command("shift"), Summary("Messes with image color channels.")]
		public async Task<RuntimeResult> Shift()
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.Shift(url);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}

		[Command("shuffle"), Summary("Shuffle gif frames.")]
		public async Task<RuntimeResult> Shuffle(string url)
		{
			using Image<Rgb24> image = await this.http.GetGifAsync<Rgb24>(url);
			if (image != null)
			{
				int frames = image.Frames.Count;

				for (int i = 0; i < frames; i++)
				{
					image.Frames.MoveFrame(i, this.random.Next(0, frames));
				}

				await using MemoryStream stream = new MemoryStream();
				await image.SaveAsGifAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);

				await this.ReplyFileAsync(stream, "shuffled.gif");

				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("LoadFailed", "Failed to load image.");
		}

		[Command("shuffle"), Summary("Shuffle gif frames.")]
		public async Task<RuntimeResult> Shuffle()
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.Shuffle(url);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}

		[Command("obama"), Summary("Obama watching TV.")]
		public async Task<RuntimeResult> Obama(string url)
		{
			using Image<Rgba32> tvImage = await this.http.GetImageAsync<Rgba32>(url);
			using Image<Rgba32> obamaImage = Image.Load<Rgba32>("./Media/obama.png");
			
			if (tvImage != null && obamaImage != null)
			{
				tvImage.Mutate(i =>
				{
					i.Resize(new ResizeOptions()
					{
						Mode = ResizeMode.Stretch,
						Size = new Size(488, 270)
					});

					i.Skew(0.12f, -0.3f);
					i.Skew(-0.4f, 0.6f); //16
					i.Skew(0.21f, 0);
					i.Rotate(0.5f);
				});

				obamaImage.Mutate(i => i.DrawImage(tvImage, new Point(415, 51), 1f));

				await using MemoryStream stream = new MemoryStream();
				await obamaImage.SaveAsJpegAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);

				await this.ReplyFileAsync(stream, "test.jpg");
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load image.");
		}

		[Command("obama"), Summary("Obama watching TV.")]
		public async Task<RuntimeResult> Obama()
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.Obama(url);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}

		[Command("jar"), Summary("Jar somebody.")]
		public async Task<RuntimeResult> Jar(IUser user)
		{
			using Image<Rgba32> avatarImage = await this.http.GetImageAsync<Rgba32>(user.GetAvatarUrl());
			using Image<Rgba32> jarImage = Image.Load<Rgba32>("./Media/jar.png");
			using Image<Rgba32> baseImage = new Image<Rgba32>(jarImage.Width, jarImage.Height);

			if (avatarImage != null)
			{
				avatarImage.Mutate(x =>
				{
					int d = Math.Max(jarImage.Width, jarImage.Height);
					
					x.Resize(new ResizeOptions()
					{
						Mode = ResizeMode.Stretch,
						Size = new Size(d / 2, d / 2)
					});
				});
				
				baseImage.Mutate(x =>
				{
					//var p = new Point((baseImage.Width / 2) - (avatarImage.Width / 2), (baseImage.Height / 2) - (avatarImage.Height / 2));
					x.DrawImage(avatarImage, new Point(256, 620), 1f);
					x.DrawImage(jarImage, new Point(0, 0), 1f);
				});
				
				await using MemoryStream stream = new MemoryStream();
				await baseImage.SaveAsPngAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);

				await this.ReplyFileAsync(stream, "jar.png");
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load image.");
		}

		[Command("deepfry"), Summary("Deepfry images.")]
		private async Task<RuntimeResult> Deepfry(string url)
		{
			using Image image = await this.http.GetMediaAsync(url);

			if (image != null)
			{
				image.Mutate(x =>
				{
					x.Brightness(0.33f);
					x.Saturate(10);
					x.GaussianSharpen(5);
				
					(int w, int h) = x.GetCurrentSize();
					x.Resize(w - (int)(w * 0.80), h - (int)(h * 0.80), KnownResamplers.NearestNeighbor, false);
					x.Resize(w, h, KnownResamplers.NearestNeighbor, false);
				});
				
				await using MemoryStream stream = new MemoryStream();

				string ext = await image.SaveAsGifOrJpegAsync(stream, jpegEncoder: new JpegEncoder()
				{
					Quality = 10
				});

				stream.Seek(0, SeekOrigin.Begin);
				await this.ReplyFileAsync(stream, "fried" + ext);
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load image.");
		}

		[Command("deepfry"), Summary("Deepfry images.")]
		private async Task<RuntimeResult> Deepfry()
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.Deepfry(url);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}

		[Command("ocr"), Summary("Get text inside image.")]
		public async Task<RuntimeResult> OCR(string url = "")
		{
			OCRSpaceResult ocr = await http.GetJsonAsync<OCRSpaceResult>($"https://api.ocr.space/parse/imageurl?apikey={config.OCRSpaceKey}&url={url}");

			if (ocr is not null)
			{
				StringBuilder builder = new StringBuilder();
				
				foreach (OCRParsedResult result in ocr.ParsedResults)
				{
					builder.AppendLine(result.ParsedText);
				}
				
				await ReplyCodeAsync(builder.ToString());
				
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromError("NoText", "Unable to find text in that image.");
		}

		[Command("ocr"), Summary("Get text inside image.")]
		public async Task<RuntimeResult> OCR()
		{
			if (TryGetAttachmentUrl(out string url))
			{
				return await this.OCR(url);
			}

			return MayflyResult.FromUserError("InvalidAttachment", "Attachment provided by user is invalid.");
		}
	}
}