using InvoiceSubscriber.Console.Abstractions;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace InvoiceSubscriber.ConsumerTests.Fakes
{
	public sealed class FakeInboxStore : IInboxStore
	{
		private readonly ConcurrentDictionary<string, bool> _processed = new();

		public string LastExistsKey { get; private set; }
		public string LastProcessedKey { get; private set; }
		public bool ExistsReturnValue { get; set; } = false;

		public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
		{
			LastExistsKey = key;
			return Task.FromResult(ExistsReturnValue || _processed.ContainsKey(key));
		}

		public Task MarkProcessedAsync(string key, CancellationToken ct = default)
		{
			LastProcessedKey = key;
			_processed[key] = true;
			return Task.CompletedTask;
		}

		public bool WasMarkedProcessed(string key) => _processed.ContainsKey(key);
	}
}
