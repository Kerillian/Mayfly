using Mayfly.Akinator.Enumerations;

namespace Mayfly.Akinator
{
	public interface IAkinatorServer
	{
		Language Language { get; }
		ServerType ServerType { get; }
		string BaseId { get; }
		string ServerUrl { get; }
	}
}