using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Events;
using InvoiceSubscriber.Console.Inbox;

namespace InvoiceSubscriber.Console.Consumers
{
	public class InvoiceSubmittedConsumer : IConsumer<InvoiceSubmittedEvent>
	{
		private readonly IInboxStore _inbox;
		private readonly ILogger<InvoiceSubmittedConsumer> _logger;

		public InvoiceSubmittedConsumer(IInboxStore inbox, ILogger<InvoiceSubmittedConsumer> logger)
		{
			_inbox = inbox;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<InvoiceSubmittedEvent> context)
		{
			// Stable key for the message – prefer MessageId from the Bus; otherwise build from Correlation/DomainKey
			var key = context.MessageId?.ToString("D")
			?? context.CorrelationId?.ToString("D")
			?? $"InvoiceSubmittedEvent::{context.Message.InvoiceId}";


			if (await _inbox.ExistsAsync(key, context.CancellationToken))
			{
				_logger.LogInformation("Skip duplicate message {Key}", key);
				return;
			}


			// --- Processing logic ---
			var msg = context.Message;
			_logger.LogInformation("Processing InvoiceSubmittedEvent {Id} Supplier={Supplier} Due={Due}", msg.InvoiceId, msg.Supplier, msg.DueDate);
			foreach (var l in msg.Lines)
				_logger.LogInformation(" Line: {Desc} Price={Price} Qty={Qty}", l.Description, l.Price, l.Quantity);
			// TODO: Projection to Read-DB (Mongo/Elastic)


			// Only after success
			await _inbox.MarkProcessedAsync(key, context.CancellationToken);
			_logger.LogInformation("Marked processed: {Key}", key);
		}
	}
}
