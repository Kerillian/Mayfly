using System.Net.Http.Headers;
using Mayfly.Structures;

namespace Mayfly.Services
{
	public class RedGifsService : HttpService
	{
		private const string HOST = "https://api.redgifs.com";
		
		public RedGifsService()
		{
			DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			DefaultRequestHeaders.UserAgent.Clear();
			DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/110.0");
		}
		
		private async Task SetGuestToken()
		{
			RedGifsTemporary temporary = await GetJsonAsync<RedGifsTemporary>(HOST + "/v2/auth/temporary");

			if (temporary is not null)
			{
				DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", temporary.Token);
			}
		}

		public async Task<RedgifsResult> Search(string text = "", string order = "recent", int count = 10, int page = 1)
		{
			if (DefaultRequestHeaders.Authorization is null)
			{
				await SetGuestToken();
			}

			return await GetJsonAsync<RedgifsResult>(HOST + $"/v2/gifs/search?search_text={text}&order={order}&count={count}&page={page}");
		}
	}
}