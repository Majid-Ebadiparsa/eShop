using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using InvoiceSubscriber.Console.Inbox;

public class InMemoryInboxStore : IInboxStore
{
	private readonly ConcurrentDictionary<string, bool> _set = new();

	public Task<bool> ExistsAsync(string messageId, CancellationToken ct = default)
			=> Task.FromResult(_set.ContainsKey(messageId));

	public Task MarkProcessedAsync(string messageId, CancellationToken ct = default)
	{
		_set.TryAdd(messageId, true);
		return Task.CompletedTask;
	}
}
