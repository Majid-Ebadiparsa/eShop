using FluentAssertions;
using InvoiceService.Application.Abstractions;
using InvoiceService.Application.Invoices.Commands;
using InvoiceService.Domain.Entities;
using InvoiceService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InvoiceService.Tests.Application
{
	public class SubmitInvoiceCommandTests
	{
		[Fact]
		public async Task Should_Create_Invoice_And_Publish_Event()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseSqlite("DataSource=:memory:")
			.Options;

			// Arrange: mock the repository
			var mockRepository = new Mock<IInvoiceRepository>();
			mockRepository.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
										.Returns(Task.CompletedTask);
			mockRepository.Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
										.Returns(Task.CompletedTask);

			// Arrange: mock the publisher
			var mockPublisher = new Mock<IEventPublisher>();

			var publish = new Mock<IPublishEndpoint>();
			var handler = new SubmitInvoiceCommandHandler(mockRepository.Object, mockPublisher.Object);

			var cmd = new SubmitInvoiceCommand(
				"Test Invoice",
				DateTime.UtcNow.AddDays(7),
				"ACME GmbH",
				new() { new SubmitInvoiceLine("Item A", 10.0, 2) }
			);

			var id = await handler.Handle(cmd, CancellationToken.None);
			id.Should().NotBe(Guid.Empty);

			(await mockPublisher.CountAsync(It.IsAny<CancellationToken>())).Should().Be(1);
			publish.Verify(p => p.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}
