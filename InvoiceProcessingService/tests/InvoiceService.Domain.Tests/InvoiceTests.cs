using FluentAssertions;
using InvoiceService.Domain.Entities;

namespace InvoiceService.Domain.Tests
{
	public class InvoiceTests
	{
		[Fact]
		public void Creating_Invoice_Without_Lines_Should_Throw()
		{
			var act = () => new Invoice("desc", DateTime.UtcNow.AddDays(2), "ACME", Enumerable.Empty<InvoiceLine>());
			act.Should().Throw<ArgumentException>().WithMessage("*At least one line*");
		}

		[Fact]
		public void Creating_InvoiceLine_With_NonPositive_Values_Should_Throw()
		{
			Action p = () => new InvoiceLine("x", 0, 1);
			Action q = () => new InvoiceLine("x", 1, 0);
			p.Should().Throw<ArgumentOutOfRangeException>();
			q.Should().Throw<ArgumentOutOfRangeException>();
		}

		[Fact]
		public void Valid_Invoice_Should_Be_Created()
		{
			var line = new InvoiceLine("A", 1.5, 2);
			var inv = new Invoice("desc", DateTime.UtcNow.AddDays(3), "ACME", new[] { line });

			inv.Id.Should().NotBe(Guid.Empty);
			inv.Lines.Should().HaveCount(1);
		}
	}
}
