namespace BlazorApp.Client.Utils;

public class Debouncer
{
	private Timer? _timer;
	private readonly object _lock = new object();

	public void Debounce(int milliseconds, Action action)
	{
		lock (_lock)
		{
			_timer?.Dispose();
			_timer = new Timer(_ => action(), null, milliseconds, Timeout.Infinite);
		}
	}

	public void Dispose()
	{
		lock (_lock)
		{
			_timer?.Dispose();
		}
	}
}