using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using InvoiceSubscriber.Console.Abstractions;
using Shared.Contracts.Events;

namespace InvoiceSubscriber.Console.Messaging.Consumers
{
	public class InvoiceSubmittedConsumer : IConsumer<InvoiceSubmitted>
	{
		private readonly IInboxStore _inbox;
		private readonly ILogger<InvoiceSubmittedConsumer> _logger;

		public InvoiceSubmittedConsumer(IInboxStore inbox, ILogger<InvoiceSubmittedConsumer> logger)
		{
			_inbox = inbox ?? throw new ArgumentNullException(nameof(inbox));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<InvoiceSubmitted> context)
		{
			var key =
					context.MessageId?.ToString("D")
					?? context.CorrelationId?.ToString("D")
					?? $"InvoiceSubmitted::{context.Message.InvoiceId}";

			if (await _inbox.ExistsAsync(key, context.CancellationToken))
			{
				_logger.LogInformation("Skip duplicate message {Key}", key);
				return;
			}

			var msg = context.Message;
			_logger.LogInformation("Processing Invoice {Id}, Supplier={Supplier}, Due={Due}",
					msg.InvoiceId, msg.Supplier, msg.DueDate);

			foreach (var l in msg.Lines)
				_logger.LogInformation(" Line: {Desc} | Price={Price} | Qty={Qty}",
						l.Description, l.Price, l.Quantity);

			// TODO: Projection to Read-DB (Mongo/Elastic) in future

			await _inbox.MarkProcessedAsync(key, context.CancellationToken);
			_logger.LogInformation("Marked processed: {Key}", key);
		}
	}
}
