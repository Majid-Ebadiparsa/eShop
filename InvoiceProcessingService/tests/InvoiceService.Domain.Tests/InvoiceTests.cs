using FluentAssertions;
using InvoiceService.Domain.Entities;

namespace InvoiceService.Domain.Tests;

public class InvoiceTests
{
	private static InvoiceLine L(string d = "item", double p = 1.0, int q = 1)
			=> new InvoiceLine(d, p, q);

	[Fact]
	public void Ctor_Should_Create_Invoice_When_Inputs_Are_Valid()
	{
		// Arrange
		var description = "Office Order";
		var supplier = "ACME GmbH";
		var due = DateTime.UtcNow.AddDays(7);
		var lines = new List<InvoiceLine> { L("A", 2.5, 2), L("B", 1.2, 5) };

		// Act
		var invoice = new Invoice(description, due, supplier, lines);

		// Assert
		invoice.Id.Should().NotBe(Guid.Empty);
		invoice.Description.Should().Be(description);
		invoice.Supplier.Should().Be(supplier);
		invoice.DueDate.Should().BeCloseTo(due, precision: TimeSpan.FromSeconds(1));
		invoice.Lines.Should().HaveCount(lines.Count);

		// Make sure Lines is read-only
		var coll = (ICollection<InvoiceLine>)invoice.Lines;
		coll.IsReadOnly.Should().BeTrue("Lines must be read-only");
		Action mutate = () => coll.Add(L());
		mutate.Should().Throw<NotSupportedException>();
		((object)invoice.Lines).Should().NotBeOfType<List<InvoiceLine>>();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Ctor_Should_Throw_When_Description_Is_NullOrWhiteSpace(string? bad)
	{
		Action act = () => new Invoice(bad!, DateTime.UtcNow, "Supplier", new[] { L() });
		act.Should().Throw<ArgumentException>()
			 .WithParameterName("description");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Ctor_Should_Throw_When_Supplier_Is_NullOrWhiteSpace(string? bad)
	{
		Action act = () => new Invoice("desc", DateTime.UtcNow, bad!, new[] { L() });
		act.Should().Throw<ArgumentException>()
			 .WithParameterName("supplier");
	}

	[Fact]
	public void Ctor_Should_Throw_When_Lines_Is_Null()
	{
		Action act = () => new Invoice("desc", DateTime.UtcNow, "supp", null!);
		act.Should().Throw<ArgumentException>()
			 .WithParameterName("lines");
	}

	[Fact]
	public void Ctor_Should_Throw_When_Lines_Is_Empty()
	{
		Action act = () => new Invoice("desc", DateTime.UtcNow, "supp", Enumerable.Empty<InvoiceLine>());
		act.Should().Throw<ArgumentException>()
			 .WithParameterName("lines");
	}

	[Fact]
	public void Ctor_Should_Trim_Description_And_Supplier()
	{
		var invoice = new Invoice("  desc  ", DateTime.UtcNow, "  supp  ", new[] { L() });

		invoice.Description.Should().Be("desc");
		invoice.Supplier.Should().Be("supp");
	}
}
