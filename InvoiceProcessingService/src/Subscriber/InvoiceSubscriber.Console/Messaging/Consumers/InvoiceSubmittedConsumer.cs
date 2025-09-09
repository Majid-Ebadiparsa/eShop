using InvoiceSubscriber.Console.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Abstraction.Read;
using Shared.Contracts.Events;
using Shared.Contracts.Invoices.Queries.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SysConsole = System.Console;

namespace InvoiceSubscriber.Console.Messaging.Consumers
{
	public class InvoiceSubmittedConsumer : IConsumer<InvoiceSubmitted>
	{
		private readonly IInvoiceReadRepository _invoiceReadRepository;
		private readonly IInboxStore _inbox;
		private readonly ILogger<InvoiceSubmittedConsumer> _logger;

		public InvoiceSubmittedConsumer(IInboxStore inbox, ILogger<InvoiceSubmittedConsumer> logger, IInvoiceReadRepository invoiceReadRepository)
		{
			_inbox = inbox ?? throw new ArgumentNullException(nameof(inbox));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_invoiceReadRepository = invoiceReadRepository ?? throw new ArgumentNullException(nameof(invoiceReadRepository));
		}

		public async Task Consume(ConsumeContext<InvoiceSubmitted> context)
		{
			var msg = context.Message;

			var key =
					context.MessageId?.ToString("D")
					?? context.CorrelationId?.ToString("D")
					?? $"InvoiceSubmitted::{msg.InvoiceId}";

			if (await _inbox.ExistsAsync(key, context.CancellationToken))
			{
				_logger.LogInformation("Skip duplicate message {Key}", key);
				return;
			}

			var linesCount = msg.Lines?.Count ?? 0;
			_logger.LogInformation("Processing Invoice {Id}, Supplier={Supplier}, Due={Due}, Lines={Count}", msg.InvoiceId, msg.Supplier, msg.DueDate, linesCount);

			var readModel = MapToReadModel(msg);

			if (msg.Lines is not null && msg.Lines.Count > 0)
			{
				foreach (var l in msg.Lines)
				{
					if (l is null) continue;

					_logger.LogInformation(" Line: {Desc} | Price={Price} | Qty={Qty}", l.Description, l.Price, l.Quantity);
				}
			}

			// Store to MongoDB
			await _invoiceReadRepository.AddAsync(readModel);
			_logger.LogInformation("Invoice {InvoiceId} projected to MongoDB", msg.InvoiceId);

			// Mark message as processed
			await _inbox.MarkProcessedAsync(key, context.CancellationToken);
			_logger.LogInformation("Marked processed: {Key}", key);
			SysConsole.WriteLine($"Marked processed for key: {key} => [Subscriber] Invoice: {msg.Description}, Supplier: {msg.Supplier}, Due={msg.DueDate}, Lines: {linesCount}");
		}

		private InvoiceDto MapToReadModel(InvoiceSubmitted msg)
		{
			return new InvoiceDto
			{
				InvoiceNumber = msg.InvoiceId.ToString(),
				CustomerName = msg.Supplier,
				IssuedDate = msg.DueDate,
				TotalAmount = msg.Lines?.Sum(l => (l?.Price ?? 0) * (l?.Quantity ?? 1)) ?? 0
			};
		}
	}
}
