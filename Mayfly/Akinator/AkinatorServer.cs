using Mayfly.Akinator.Enumerations;

namespace Mayfly.Akinator
{
	public class AkinatorServer : IAkinatorServer
	{
		public Language Language { get; }
		public ServerType ServerType { get; }
		public string BaseId { get; }
		public string ServerUrl { get; }

		public AkinatorServer(Language language, ServerType serverType, string baseId, string serverUrls)
		{
			ServerUrl = serverUrls;
			Language = language;
			ServerType = serverType;
			BaseId = baseId;
		}
	}
}