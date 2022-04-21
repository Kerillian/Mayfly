using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Mayfly.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Mayfly.Extensions;
using Mayfly.Services;
using Mayfly.Structures;
using Mayfly.UrlBuilders;
using SixLabors.ImageSharp.Formats.Jpeg;
using Image = SixLabors.ImageSharp.Image;
using RuntimeResult = Discord.Interactions.RuntimeResult;

namespace Mayfly.Modules
{
	public class ImageModule : MayflyModule
	{
		public HttpService http { get; set; }
		public RandomService random { get; set; }
		public BotConfig config { get; set; }

		[SlashCommand("oil", "Make anything art."), RateLimit(10)]
		public async Task<RuntimeResult> Oil(string url, [MinValue(1), MaxValue(50)] int levels = 25, [MinValue(1), MaxValue(50)] int size = 30)
		{
			await DeferAsync();
			using Image image = await http.GetMediaAsync(url);

			if (image != null)
			{
				image.Mutate(x => x.OilPaint(levels, size));

				await using MemoryStream stream = new MemoryStream();
				string ext = await image.SaveAsGifOrPngAsync(stream);
				
				stream.Seek(0, SeekOrigin.Begin);
				
				await FollowupWithFileAsync(stream, "oil" + ext);
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load image.");
		}
		
		[SlashCommand("corrupt", "Corrupt jpeg image data."), ]
		public async Task<RuntimeResult> Corrupt(string url, [MinValue(5), MaxValue(100)] int iterations = 25)
		{
			await DeferAsync();
			using Image<Rgba32> image = await http.GetImageAsync<Rgba32>(url);
			
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

				for (int i = 0; i < iterations; i++)
				{
					buffer[random.Next(header + 4, buffer.Length)] = 0x00;
				}

				await using MemoryStream corruptedStream = new MemoryStream(buffer);
				await FollowupWithFileAsync(corruptedStream, "corrupted.jpg");
			}
			else
			{
				return MayflyResult.FromUserError("LoadFailed", "Failed to load image.");
			}
			
			return MayflyResult.FromSuccess();
		}
		
		[SlashCommand("crush", "Crushes the image resolution.")]
		public async Task<RuntimeResult> Crush(string url, [MinValue(0), MaxValue(1)] float scale = 0.5f, [MinValue(1), MaxValue(100)] int quality = 10)
		{
			await DeferAsync();
			using Image image = await http.GetMediaAsync(url);
			
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
				await FollowupWithFileAsync(stream, "crushed" + ext);
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("LoadFailed", "Failed to load image.");
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
		
		[SlashCommand("shift", "Bitshift image color channels.")]
		public async Task<RuntimeResult> Shift(string url)
		{
			await DeferAsync();
			using Image<Rgba32> image = await http.GetImageAsync<Rgba32>(url);
			
			if (image != null)
			{
				ColorFuckWorker(image);

				await using MemoryStream stream = new MemoryStream();
				await image.SaveAsJpegAsync(stream, new JpegEncoder()
				{
					Quality = 5
				});

				stream.Seek(0, SeekOrigin.Begin);
				await FollowupWithFileAsync(stream, "shifted.jpg");
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("InvalidUrl", "Url provided by user is invalid.");
		}

		[SlashCommand("shuffle", "Shuffle gif frames.")]
		public async Task<RuntimeResult> Shuffle(string url)
		{
			await DeferAsync();
			using Image<Rgb24> image = await http.GetGifAsync<Rgb24>(url);
			
			if (image != null)
			{
				int frames = image.Frames.Count;

				for (int i = 0; i < frames; i++)
				{
					image.Frames.MoveFrame(i, random.Next(0, frames));
				}

				await using MemoryStream stream = new MemoryStream();
				await image.SaveAsGifAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);

				await FollowupWithFileAsync(stream, "shuffled.gif");

				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromUserError("LoadFailed", "Failed to load image.");
		}
		
		[SlashCommand("obama", "Obama watching TV.")]
		public async Task<RuntimeResult> Obama(string url)
		{
			await DeferAsync();
			using Image<Rgba32> tvImage = await http.GetImageAsync<Rgba32>(url);
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

				await FollowupWithFileAsync(stream, "test.jpg");
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load image.");
		}
		
		[SlashCommand("jar", "Jar somebody.")]
		public async Task<RuntimeResult> Jar(IUser user)
		{
			await DeferAsync();
			using Image avatarImage = await http.GetMediaAsync(user.GetAvatarUrl());
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
					x.DrawImage(avatarImage, new Point(256, 620), 1f);
					x.DrawImage(jarImage, new Point(0, 0), 1f);
				});
				
				await using MemoryStream stream = new MemoryStream();
				await baseImage.SaveAsPngAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);

				await FollowupWithFileAsync(stream, "jar.png");
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load avatar.");
		}

		[UserCommand("jar")]
		public async Task JarContext(IUser user)
		{
			await Jar(user);
		}

		// Avatar = 135
		// wilson = 72, 301
		// cuomo  = 349, 363
		// sharp  = 604, 321
		// bell   = 857, 336
		// These are not magic numbers! I used image editing software to draw squares and figure out where to draw the avatars.
		[SlashCommand("weezer", "Weezer moment. (Thanks Allie, and Nykane <3)")]
		public async Task<RuntimeResult> Weezer(IUser wilson, IUser cuomo, IUser sharp, IUser bell)
		{
			await DeferAsync();
			using Image wilsonImage = await http.GetMediaAsync(wilson.GetAvatarUrl());
			using Image cuomoImage = await http.GetMediaAsync(cuomo.GetAvatarUrl());
			using Image sharpImage = await http.GetMediaAsync(sharp.GetAvatarUrl());
			using Image bellImage = await http.GetMediaAsync(bell.GetAvatarUrl());
			
			using Image<Rgba32> weezerImage = Image.Load<Rgba32>("./Media/weezer.png");
			using Image<Rgba32> baseImage = new Image<Rgba32>(weezerImage.Width, weezerImage.Height);

			if (wilsonImage != null && cuomoImage != null && sharpImage != null && bellImage != null)
			{
				wilsonImage.Mutate(x => x.Resize(135, 135));
				cuomoImage.Mutate(x => x.Resize(135, 135));
				sharpImage.Mutate(x => x.Resize(135, 135));
				bellImage.Mutate(x => x.Resize(135, 135));
				
				baseImage.Mutate(x =>
				{
					x.DrawImage(wilsonImage, new Point(72, 301), 1f);
					x.DrawImage(cuomoImage, new Point(349, 363), 1f);
					x.DrawImage(sharpImage, new Point(604, 321), 1f);
					x.DrawImage(bellImage, new Point(857, 336), 1f);
					
					x.DrawImage(weezerImage, new Point(0, 0), 1f);
				});
				
				await using MemoryStream stream = new MemoryStream();
				await baseImage.SaveAsPngAsync(stream);
				stream.Seek(0, SeekOrigin.Begin);

				await FollowupWithFileAsync(stream, "weezer.png");
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromError("LoadFailed", "Failed to load avatar(s).");
		}
		
		[SlashCommand("deepfry", "Deepfry images.")]
		private async Task<RuntimeResult> Deepfry(string url)
		{
			await DeferAsync();
			using Image image = await http.GetMediaAsync(url);

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
				await FollowupWithFileAsync(stream, "fried" + ext);
				
				return MayflyResult.FromSuccess();
			}

			return MayflyResult.FromError("LoadFailed", "Failed to load image.");
		}
		
		[SlashCommand("ocr", "Get text inside image.")]
		public async Task<RuntimeResult> OCR(string url)
		{
			await DeferAsync();
			OCRSpaceResult ocr = await http.GetJsonAsync<OCRSpaceResult>(new OcrSpaceBuilder(config.OCRSpaceKey).WithUrl(url).Build());
				
			if (ocr is not null)
			{
				StringBuilder builder = new StringBuilder();
				
				foreach (OCRParsedResult result in ocr.ParsedResults)
				{
					builder.AppendLine(result.ParsedText);
				}
				
				await FollowupWithCodeAsync(builder.ToString());
				
				return MayflyResult.FromSuccess();
			}
			
			return MayflyResult.FromError("NoText", "Unable to find text in that image.");
		}
	}
}