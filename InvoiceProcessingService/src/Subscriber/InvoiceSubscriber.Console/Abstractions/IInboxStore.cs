using System.Threading;
using System.Threading.Tasks;

namespace InvoiceSubscriber.Console.Abstractions
{
	public interface IInboxStore
	{
		Task<bool> ExistsAsync(string messageId, CancellationToken ct = default);
		Task MarkProcessedAsync(string messageId, CancellationToken ct = default);
	}
}
