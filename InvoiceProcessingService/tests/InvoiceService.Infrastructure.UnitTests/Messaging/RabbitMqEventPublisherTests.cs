using InvoiceService.Infrastructure.Messaging;
using MassTransit;
using Moq;
using Shared.Contracts.Events;

namespace InvoiceService.Infrastructure.UnitTests.Messaging
{
	public class RabbitMqEventPublisherTests
	{
		[Fact]
		public async Task Should_Publish_Event_To_IPublishEndpoint_With_CancellationToken()
		{
			var publishEndpoint = new Mock<IPublishEndpoint>(MockBehavior.Strict);
			var sut = new RabbitMqEventPublisher(publishEndpoint.Object);

			var evt = new InvoiceSubmitted
			{
				InvoiceId = Guid.NewGuid(),
				Description = "Office supplies",
				Supplier = "ACME GmbH",
				DueDate = DateTime.UtcNow.AddDays(3),
				Lines = new List<InvoiceLineItem>
				{
						new InvoiceLineItem { Description = "Paper", Price = 5.99, Quantity = 10 }
				}
			};


			var ct = new CancellationTokenSource().Token;

			publishEndpoint
					.Setup(p => p.Publish(evt, ct))
					.Returns(Task.CompletedTask)
					.Verifiable();

			await sut.PublishInvoiceSubmittedAsync(evt, ct);

			publishEndpoint.Verify(p => p.Publish(evt, ct), Times.Once);
		}
	}

}
