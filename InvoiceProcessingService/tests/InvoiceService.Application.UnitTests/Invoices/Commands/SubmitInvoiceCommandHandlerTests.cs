using FluentAssertions;
using InvoiceService.Application.Abstractions;
using InvoiceService.Application.Invoices.Commands;
using InvoiceService.Application.UnitTests.Builders;
using InvoiceService.Domain.Entities;
using Moq;
using SharedService.Contracts.Events.Invoice;

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

			// Capture published event parameters
			Guid? publishedInvoiceId = null;
			string? publishedDescription = null;
			DateTime? publishedDueDate = null;
			string? publishedSupplier = null;
			IReadOnlyList<InvoiceLineItem>? publishedLines = null;
			
			publisher.Setup(p => p.PublishInvoiceSubmittedAsync(
				It.IsAny<Guid>(),
				It.IsAny<string>(),
				It.IsAny<DateTime>(),
				It.IsAny<string>(),
				It.IsAny<IReadOnlyList<InvoiceLineItem>>(),
				It.IsAny<Guid>(),
				It.IsAny<CancellationToken>()))
			.Callback<Guid, string, DateTime, string, IReadOnlyList<InvoiceLineItem>, Guid, CancellationToken>(
				(invId, desc, due, sup, lines, corr, _) =>
				{
					publishedInvoiceId = invId;
					publishedDescription = desc;
					publishedDueDate = due;
					publishedSupplier = sup;
					publishedLines = lines;
				})
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

			publishedInvoiceId.Should().NotBeNull();
			publishedInvoiceId.Should().Be(id);
			publishedDescription.Should().Be(cmd.Description);
			publishedSupplier.Should().Be(cmd.Supplier);
			publishedDueDate.Should().Be(cmd.DueDate);
			publishedLines.Should().NotBeNull();
			publishedLines.Should().HaveCount(cmd.Lines.Count);

			repo.Verify(r => r.AddAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Once);
			publisher.Verify(p => p.PublishInvoiceSubmittedAsync(
				It.IsAny<Guid>(),
				It.IsAny<string>(),
				It.IsAny<DateTime>(),
				It.IsAny<string>(),
				It.IsAny<IReadOnlyList<InvoiceLineItem>>(),
				It.IsAny<Guid>(),
				It.IsAny<CancellationToken>()), Times.Once);
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

			IReadOnlyList<InvoiceLineItem>? publishedLines = null;
			publisher.Setup(p => p.PublishInvoiceSubmittedAsync(
				It.IsAny<Guid>(),
				It.IsAny<string>(),
				It.IsAny<DateTime>(),
				It.IsAny<string>(),
				It.IsAny<IReadOnlyList<InvoiceLineItem>>(),
				It.IsAny<Guid>(),
				It.IsAny<CancellationToken>()))
			.Callback<Guid, string, DateTime, string, IReadOnlyList<InvoiceLineItem>, Guid, CancellationToken>(
				(_, _, _, _, lines, _, _) => publishedLines = lines)
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
			publishedLines.Should().NotBeNull();
			publishedLines.Should().HaveCount(2);
			publishedLines![0].Description.Should().Be("Paper");
			publishedLines[0].Price.Should().Be(5.99m);
			publishedLines[0].Quantity.Should().Be(10);
			publishedLines[1].Description.Should().Be("Pen");
			publishedLines[1].Price.Should().Be(1.99m);
			publishedLines[1].Quantity.Should().Be(5);
		}
	}
}
