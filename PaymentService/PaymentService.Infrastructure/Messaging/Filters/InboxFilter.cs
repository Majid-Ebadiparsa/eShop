using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Messaging.Filters
{
	public class InboxFilter<T> : IFilter<ConsumeContext<T>> where T : class
	{
		private readonly IDbContextFactory<PaymentDbContext> _dbFactory;
		private readonly string _consumerName;

		public InboxFilter(IDbContextFactory<PaymentDbContext> dbFactory, string consumerName)
				=> (_dbFactory, _consumerName) = (dbFactory, consumerName);

		public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
		{
			if (!context.Headers.TryGetHeader("MessageId", out var _))
			{
				// Masstransit has always MessageId; this fallback is for safety:
			}

			var messageId = context.MessageId ?? NewId.NextGuid();
			await using var db = await _dbFactory.CreateDbContextAsync(context.CancellationToken);

			var exists = await db.Set<InboxMessage>().AnyAsync(x => x.Id == messageId && x.Consumer == _consumerName, context.CancellationToken);
			if (exists)
			{
				// Ignore running the duplicate consumer
				return;
			}

			await next.Send(context); // Consumer do its work

			db.Add(new InboxMessage { Id = messageId, Consumer = _consumerName });
			await db.SaveChangesAsync(context.CancellationToken);
		}

		public void Probe(ProbeContext context) => context.CreateFilterScope("inboxFilter");
	}
}
