using Mayfly.Structures;

namespace Mayfly.Services
{
	public class NookipediaService : HttpClient
	{
		private const string Endpoint = "https://api.nookipedia.com/";

		public NookipediaService(BotConfig bc)
		{
			DefaultRequestHeaders.Add("X-API-KEY", bc.NookipediaKey);
		}

		public List<VillagerJson> GetVillagers(string name, string species = null, string personality = null, string game = null)
		{
			return null;
		}
	}
}