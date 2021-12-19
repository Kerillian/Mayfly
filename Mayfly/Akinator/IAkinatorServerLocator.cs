using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mayfly.Akinator.Enumerations;

namespace Mayfly.Akinator
{
	public interface IAkinatorServerLocator
	{
		Task<IAkinatorServer> SearchAsync(Language language, ServerType serverType, CancellationToken cancellationToken = default);
		Task<IEnumerable<IAkinatorServer>> SearchAllAsync(Language language, CancellationToken cancellationToken = default);
	}
}