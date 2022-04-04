using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using Mayfly.Structures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Mayfly.Services
{
	public class HttpService : HttpClient
	{
		// Thank you duckduckgo, you're a life saver.
		private const string ProxyUrl = "https://proxy.duckduckgo.com/iu/?u=";

		public HttpService()
		{
			this.Timeout = new TimeSpan(0, 0, 10);
			this.MaxResponseContentBufferSize = 8000000; // Limit for non-nitro users. (8mb)
		}

		public async Task<Image<T>> GetImageAsync<T>(string url) where T : unmanaged, IPixel<T>
		{
			if (url.Contains(".gif"))
			{
				return null;
			}

			try
			{
				using HttpResponseMessage response = await this.GetAsync(ProxyUrl + Uri.EscapeDataString(url));
				string mt = response.Content.Headers.ContentType?.MediaType;

				if (mt != null && response.IsSuccessStatusCode && mt.Contains("image/") && !mt.Contains("gif"))
				{
					Image<T> image = Image.Load<T>(await response.Content.ReadAsByteArrayAsync());
					image.Metadata.ExifProfile = null;
					return image;
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Image<T>> GetGifAsync<T>(string url) where T : unmanaged, IPixel<T>
		{
			try
			{
				using HttpResponseMessage response = await this.GetAsync(ProxyUrl + Uri.EscapeDataString(url));
				string mt = response.Content.Headers.ContentType?.MediaType;

				if (mt != null && response.IsSuccessStatusCode && mt.Contains("image/gif"))
				{
					return Image.Load<T>(await response.Content.ReadAsByteArrayAsync());
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		public async Task<Image> GetMediaAsync(string url)
		{
			try
			{
				using HttpResponseMessage response = await this.GetAsync(ProxyUrl + Uri.EscapeDataString(url));
				string mt = response.Content.Headers.ContentType?.MediaType;
				
				if (mt == "image/png")
				{
					return Image.Load<Rgba32>(await response.Content.ReadAsByteArrayAsync());
				}

				return Image.Load<Rgb24>(await response.Content.ReadAsByteArrayAsync());
			}
			catch
			{
				return null;
			}
		}

		public async Task<JObject> GetJObjectAsync(string url)
		{
			try
			{
				return JObject.Parse(await this.GetStringAsync(url));
			}
			catch
			{
				return null;
			}
		}

		public async Task<JArray> GetJArrayAsync(string url)
		{
			try
			{
				return JArray.Parse(await this.GetStringAsync(url));
			}
			catch
			{
				return null;
			}
		}

		#nullable enable
		public async Task<T?> GetJsonAsync<T>(string url) where T : class
		{
			try
			{
				return JsonConvert.DeserializeObject<T>(await this.GetStringAsync(url), new JsonSerializerSettings()
				{
					MissingMemberHandling = MissingMemberHandling.Ignore,
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}

		public async Task<V?> PostJsonAsync<T,V>(string url, T data) where T : class where V : class
		{
			try
			{
				StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
				HttpResponseMessage response = await this.PostAsync(url, content);

				return JsonConvert.DeserializeObject<V>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings()
				{
					MissingMemberHandling = MissingMemberHandling.Ignore,
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}

		public async Task<JObject?> GraphQL(string url, string query, Dictionary<string, string> variables)
		{
			JObject body = new JObject()
			{
				new JProperty("query", query),
				new JProperty("variables", JObject.FromObject(variables))
			};

			StringContent content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
			HttpResponseMessage response = await this.PostAsync(url, content);
			JObject data = JObject.Parse(await response.Content.ReadAsStringAsync());

			return data["data"]?.Value<JObject>();
		}
	}
}