using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mayfly.Akinator.Enumerations;
using Mayfly.Akinator.Model.External;
using Mayfly.Akinator.Utils;

namespace Mayfly.Akinator
{
	public class AkinatorServerLocator : IAkinatorServerLocator
	{
		private class ServerCache
		{
			public bool? IsHealthy { get; set; }
			public IAkinatorServer Server { get; }

			public ServerCache(IAkinatorServer server)
			{
				Server = server;
			}
		}

		private const string ServerListUrl = "https://global3.akinator.com/ws/instances_v2.php?media_id=14&footprint=cd8e6509f3420878e18d75b9831b317f&mode=https";
		private static readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);
		private readonly AkiWebClient m_webClient;
		private ICollection<ServerCache> m_cachedServers;

		public AkinatorServerLocator()
		{
			m_webClient = new AkiWebClient();
		}

		public async Task<IAkinatorServer> SearchAsync(Language language, ServerType serverType, CancellationToken cancellationToken = default)
		{
			await EnsureServersAsync(cancellationToken).ConfigureAwait(false);
			List<ServerCache> serversMatchingCriteria = m_cachedServers.Where(p => p.Server.ServerType == serverType && p.Server.Language == language).ToList();
			return await GetHealthyServersAsync(serversMatchingCriteria);
		}

		public async Task<IEnumerable<IAkinatorServer>> SearchAllAsync(Language language, CancellationToken cancellationToken = default)
		{
			await EnsureServersAsync(cancellationToken).ConfigureAwait(false);
			IEnumerable<ServerType> serverTypes = Enum.GetValues(typeof(ServerType)).Cast<ServerType>();
			
			ConcurrentBag<IAkinatorServer> serverBag = new ConcurrentBag<IAkinatorServer>();
			
			ParallelQuery<Task> tasks = serverTypes.AsParallel().Select(async serverType =>
			{
				IAkinatorServer healthyServer = await SearchAsync(language, serverType, cancellationToken).ConfigureAwait(false);
				
				if (healthyServer != null)
				{
					serverBag.Add(healthyServer);
				}
			});
			
			await Task.WhenAll(tasks);
			return serverBag.OrderBy(p => p.ServerType).ToArray();
		}

		private async Task EnsureServersAsync(CancellationToken cancellationToken)
		{
			await m_semaphoreSlim.WaitAsync(cancellationToken);
			
			try
			{
				this.m_cachedServers ??= await this.LoadServersAsync(cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				m_semaphoreSlim.Release();
			}
		}

		private async Task<IAkinatorServer> GetHealthyServersAsync(IReadOnlyCollection<ServerCache> cachedServers)
		{
			foreach (ServerCache server in cachedServers.OrderBy(p => p.IsHealthy != true))
			{
				if (server.IsHealthy == true)
				{
					return server.Server;
				}

				if (await CheckHealth(server.Server.ServerUrl).ConfigureAwait(false))
				{
					server.IsHealthy = true;
					return server.Server;
				}

				m_cachedServers.Remove(server);
			}

			return null;
		}

		private async Task<ICollection<ServerCache>> LoadServersAsync(CancellationToken cancellationToken)
		{
			HttpResponseMessage response = await m_webClient.GetAsync(ServerListUrl, cancellationToken).ConfigureAwait(false);
			string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			ServerSearchResult serverListRaw = XmlConverter.ToClass<ServerSearchResult>(content);
			
			return MapToServerListAsync(serverListRaw).Select(server => new ServerCache(server)).ToList();
		}

		private static IEnumerable<IAkinatorServer> MapToServerListAsync(ServerSearchResult serverListRaw)
		{
			List<ServerSearchResultInstance> instancesRaw = serverListRaw?.PARAMETERS?.INSTANCE;
			
			if (instancesRaw == null)
			{
				return new List<IAkinatorServer>();
			}

			List<IAkinatorServer> servers = new List<IAkinatorServer>();
			
			foreach (ServerSearchResultInstance instanceRaw in instancesRaw)
			{
				Language language = MapLanguage(instanceRaw.LANGUAGE?.LANG_ID);
				ServerType serverType = MapServerType(instanceRaw.SUBJECT?.SUBJ_ID);
				string baseId = instanceRaw.BASE_LOGIQUE_ID;

				List<string> serverUrls = new List<string>()
				{
					instanceRaw.URL_BASE_WS
				};
				
				serverUrls.AddRange(instanceRaw.CANDIDATS.URL);

				foreach (string serverUrl in serverUrls)
				{
					servers.Add(new AkinatorServer(language, serverType, baseId, serverUrl));
				}
			}

			return servers;
		}

		private async Task<bool> CheckHealth(string serverUrl)
		{
			HttpResponseMessage result = await m_webClient.GetAsync($"{serverUrl}/answer").ConfigureAwait(false);
			return result.StatusCode == HttpStatusCode.OK;
		}

		private static ServerType MapServerType(string serverTypeCode)
		{
			return serverTypeCode switch
			{
				"1" => ServerType.Person,
				"2" => ServerType.Object,
				"7" => ServerType.Place,
				"13" => ServerType.Movie,
				"14" => ServerType.Animal,
				_ => throw new NotSupportedException($"Server-Type with the code {serverTypeCode} is currently not supported.")
			};
		}

		private static Language MapLanguage(string languageCode)
		{
			return languageCode switch
			{
				"ar" => Language.Arabic,
				"cn" => Language.Chinese,
				"nl" => Language.Dutch,
				"en" => Language.English,
				"fr" => Language.French,
				"de" => Language.German,
				"id" => Language.Indonesian,
				"il" => Language.Israeli,
				"it" => Language.Italian,
				"jp" => Language.Japanese,
				"kr" => Language.Korean,
				"pl" => Language.Polski,
				"pt" => Language.Portuguese,
				"es" => Language.Spanish,
				"ru" => Language.Russian,
				"tr" => Language.Turkish,
				_ => throw new NotSupportedException($"Language with the code {languageCode} is currently not supported.")
			};
		}
	}
}