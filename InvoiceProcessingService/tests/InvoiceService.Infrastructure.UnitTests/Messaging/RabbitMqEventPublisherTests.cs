using InvoiceService.Infrastructure.Messaging;
using MassTransit;
using Moq;
using SharedService.Contracts.Events.Invoice;

namespace InvoiceService.Infrastructure.UnitTests.Messaging
{
	public class RabbitMqEventPublisherTests
	{
		[Fact]
		public async Task Should_Publish_Event_To_IPublishEndpoint_With_CancellationToken()
		{
			var publishEndpoint = new Mock<IPublishEndpoint>(MockBehavior.Strict);
			var sut = new RabbitMqEventPublisher(publishEndpoint.Object);

			var invoiceId = Guid.NewGuid();
			var description = "Office supplies";
			var dueDate = DateTime.UtcNow.AddDays(3);
			var supplier = "ACME GmbH";
			var lines = new List<InvoiceLineItem>
			{
				new InvoiceLineItem("Paper", 5.99m, 10)
			};
			var correlationId = Guid.NewGuid();
			var ct = new CancellationTokenSource().Token;

			// Setup to capture the published event
			InvoiceSubmitted? publishedEvent = null;
			publishEndpoint
				.Setup(p => p.Publish(It.IsAny<InvoiceSubmitted>(), ct))
				.Callback<InvoiceSubmitted, CancellationToken>((evt, _) => publishedEvent = evt)
				.Returns(Task.CompletedTask)
				.Verifiable();

			// Act
			await sut.PublishInvoiceSubmittedAsync(invoiceId, description, dueDate, supplier, lines, correlationId, ct);

			// Assert
			publishEndpoint.Verify(p => p.Publish(It.IsAny<InvoiceSubmitted>(), ct), Times.Once);
			
			Assert.NotNull(publishedEvent);
			Assert.Equal(invoiceId, publishedEvent.InvoiceId);
			Assert.Equal(description, publishedEvent.Description);
			Assert.Equal(dueDate, publishedEvent.DueDate);
			Assert.Equal(supplier, publishedEvent.Supplier);
			Assert.Equal(correlationId, publishedEvent.CorrelationId);
			Assert.Single(publishedEvent.Lines);
			Assert.Equal("Paper", publishedEvent.Lines[0].Description);
			Assert.Equal(5.99m, publishedEvent.Lines[0].Price);
			Assert.Equal(10, publishedEvent.Lines[0].Quantity);
		}
	}

}
