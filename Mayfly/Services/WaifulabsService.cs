using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mayfly.Structures;

namespace Mayfly.Services
{
	public class WaifulabsService
	{
		private readonly HttpService http;
		private readonly Stack<byte[]> rawImages = new Stack<byte[]>();

		public WaifulabsService(HttpService hs)
		{
			this.http = hs;
		}

		public async Task<Stream> GetImageStream()
		{
			if (0 >= this.rawImages.Count)
			{
				WaifulabsResult json = await this.http.PostJsonAsync<WaifulabsGenerate, WaifulabsResult>("https://api.waifulabs.com/generate", new WaifulabsGenerate()
				{
					Step = 0
				});

				foreach (WaifulabsGirl girl in json.NewGirls)
				{
					this.rawImages.Push(Convert.FromBase64String(girl.Image));
				}
			}

			return new MemoryStream(this.rawImages.Pop());
		}
	}
}