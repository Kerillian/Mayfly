namespace Mayfly.Utilities
{
	public class PeriodicTask
	{
		private readonly CancellationTokenSource source = new CancellationTokenSource();

		public PeriodicTask(TimeSpan delay, Func<CancellationToken, Task> action)
		{
			Task.Run(async () =>
			{
				while (!source.IsCancellationRequested)
				{
					try
					{
						await action(source.Token);
						await Task.Delay(delay, source.Token).ConfigureAwait(false);
					}
					catch
					{
						break;
					}
				}
			}).ConfigureAwait(false);
		}
		
		public void Cancel()
		{
			using (source)
			{
				source.Cancel();
			}
		}
	}
}