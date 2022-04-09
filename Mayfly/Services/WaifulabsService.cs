using Mayfly.Structures;

namespace Mayfly.Services
{
	public class WaifulabsService
	{
		private readonly HttpService http;
		private readonly Stack<byte[]> rawImages = new Stack<byte[]>();

		public WaifulabsService(HttpService hs)
		{
			http = hs;
		}

		public async Task<Stream> GetImageStream()
		{
			if (0 >= rawImages.Count)
			{
				WaifulabsResult json = await http.PostJsonAsync<WaifulabsGenerate, WaifulabsResult>("https://api.waifulabs.com/generate", new WaifulabsGenerate()
				{
					Step = 0
				});

				foreach (WaifulabsGirl girl in json.NewGirls)
				{
					rawImages.Push(Convert.FromBase64String(girl.Image));
				}
			}

			return new MemoryStream(rawImages.Pop());
		}
	}
}