using FluentAssertions;
using InvoiceService.Application.Abstractions;
using InvoiceService.Application.Invoices.Commands;
using InvoiceService.Domain.Entities;
using Shared.Contracts.Events;
using Moq;

namespace InvoiceService.Application.UnitTests
{
	public class SubmitInvoiceHandlerTests
	{
		[Fact]
		public async Task Handle_Should_Save_And_Publish()
		{
			// Arrange
			var repo = new Mock<IInvoiceRepository>();
			repo.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
					.Returns(Task.CompletedTask);

			var publisher = new Mock<IEventPublisher>();
			publisher.Setup(p => p.PublishInvoiceSubmittedAsync(It.IsAny<InvoiceSubmitted>(), It.IsAny<CancellationToken>()))
							 .Returns(Task.CompletedTask);

			var handler = new SubmitInvoiceCommandHandler(repo.Object, publisher.Object);

			var cmd = new SubmitInvoiceCommand(
					"Office", DateTime.UtcNow.AddDays(2), "ACME",
					new() { new SubmitInvoiceLine("Pens", 1.2, 10) });

			// Act
			var id = await handler.Handle(cmd, CancellationToken.None);

			// Assert
			id.Should().NotBe(Guid.Empty);
			repo.Verify(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
			publisher.Verify(p => p.PublishInvoiceSubmittedAsync(It.IsAny<InvoiceSubmitted>(), It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
