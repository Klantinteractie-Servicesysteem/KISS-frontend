namespace Kiss.Elastic.Sync
{
	public static class Helpers
	{
		public static void CancelSafely(this CancellationTokenSource source)
		{
			try
			{
				source.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}
		}

		public static string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process) ?? throw new Exception("missing environment variable: " + name);
	}
}
