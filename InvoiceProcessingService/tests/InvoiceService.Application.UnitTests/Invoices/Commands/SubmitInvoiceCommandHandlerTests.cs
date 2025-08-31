using FluentAssertions;
using InvoiceService.Application.Abstractions;
using InvoiceService.Application.Invoices.Commands;
using InvoiceService.Application.UnitTests.Builders;
using InvoiceService.Domain.Entities;
using Moq;
using Shared.Contracts.Events;

namespace InvoiceService.Application.UnitTests.Invoices.Commands
{
	public class SubmitInvoiceCommandHandlerTests
	{
		[Fact]
		public async Task Should_Persist_And_Publish_And_Return_Id()
		{
			// Arrange
			var repo = new Mock<IInvoiceRepository>();
			var publisher = new Mock<IEventPublisher>();

			// Capture the invoice passed to repository
			Invoice? saved = null;
			repo.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
					.Callback<Invoice, CancellationToken>((inv, _) =>
					{
						saved = inv;
						if (inv.Id == Guid.Empty)
							inv.GetType().GetProperty("Id")?.SetValue(inv, Guid.NewGuid());
					})
					.Returns(Task.CompletedTask);

			// Capture published event
			InvoiceSubmitted? published = null;
			publisher.Setup(p => p.PublishInvoiceSubmittedAsync(It.IsAny<InvoiceSubmitted>(), It.IsAny<CancellationToken>()))
							 .Callback<InvoiceSubmitted, CancellationToken>((evt, _) => published = evt)
							 .Returns(Task.CompletedTask);

			var sut = new SubmitInvoiceCommandHandler(repo.Object, publisher.Object);
			var cmd = new SubmitInvoiceCommandBuilder().Build();

			// Act
			var id = await sut.Handle(cmd, CancellationToken.None);

			// Assert
			id.Should().NotBe(Guid.Empty);

			saved.Should().NotBeNull();
			saved!.Description.Should().Be(cmd.Description);
			saved.Supplier.Should().Be(cmd.Supplier);
			saved.DueDate.Should().Be(cmd.DueDate);
			saved.Lines.Should().HaveCount(cmd.Lines.Count);

			published.Should().NotBeNull();
			published!.InvoiceId.Should().Be(id);
			published.Description.Should().Be(cmd.Description);
			published.Supplier.Should().Be(cmd.Supplier);
			published.DueDate.Should().Be(cmd.DueDate);
			published.Lines.Should().NotBeNull();
			published.Lines.Should().HaveCount(cmd.Lines.Count);

			repo.Verify(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
			publisher.Verify(p => p.PublishInvoiceSubmittedAsync(It.IsAny<InvoiceSubmitted>(), It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task Should_Map_Line_Items_Correctly()
		{
			// Arrange
			var repo = new Mock<IInvoiceRepository>();
			var publisher = new Mock<IEventPublisher>();

			Invoice? saved = null;
			repo.Setup(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()))
					.Callback<Invoice, CancellationToken>((inv, _) =>
					{
						saved = inv;
						if (inv.Id == Guid.Empty)
							inv.GetType().GetProperty("Id")?.SetValue(inv, Guid.NewGuid());
					})
					.Returns(Task.CompletedTask);

			InvoiceSubmitted? published = null;
			publisher.Setup(p => p.PublishInvoiceSubmittedAsync(It.IsAny<InvoiceSubmitted>(), It.IsAny<CancellationToken>()))
							 .Callback<InvoiceSubmitted, CancellationToken>((evt, _) => published = evt)
							 .Returns(Task.CompletedTask);

			var sut = new SubmitInvoiceCommandHandler(repo.Object, publisher.Object);
			var cmd = new SubmitInvoiceCommand(
					"Office supplies",
					DateTime.UtcNow.AddDays(3),
					"ACME GmbH",
					new List<SubmitInvoiceLine>
					{
						new("Paper", 5.99, 10),
						new("Pen",   1.99, 5)
					});

			// Act
			_ = await sut.Handle(cmd, CancellationToken.None);

			// Assert
			published.Should().NotBeNull();
			published!.Lines.Should().HaveCount(2);
			published.Lines[0].Description.Should().Be("Paper");
			published.Lines[0].Price.Should().Be(5.99);
			published.Lines[0].Quantity.Should().Be(10);
			published.Lines[1].Description.Should().Be("Pen");
			published.Lines[1].Price.Should().Be(1.99);
			published.Lines[1].Quantity.Should().Be(5);
		}
	}
}
