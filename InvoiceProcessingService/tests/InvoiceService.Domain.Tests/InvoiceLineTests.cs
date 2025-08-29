using FluentAssertions;
using InvoiceService.Domain.Entities;

namespace InvoiceService.Domain.Tests;

public class InvoiceLineTests
{
	[Fact]
	public void Ctor_Should_Create_InvoiceLine_When_Inputs_Are_Valid()
	{
		// Arrange
		var desc = "Item A";
		var price = 12.5;
		var qty = 3;

		// Act
		var line = new InvoiceLine(desc, price, qty);

		// Assert
		line.Id.Should().NotBe(Guid.Empty);
		line.Description.Should().Be(desc);
		line.Price.Should().Be(price);
		line.Quantity.Should().Be(qty);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Ctor_Should_Throw_When_Description_Is_NullOrWhiteSpace(string? bad)
	{
		Action act = () => new InvoiceLine(bad!, 10.0, 1);
		act.Should().Throw<ArgumentException>()
			 .WithParameterName("description");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-0.0001)]
	[InlineData(-10)]
	public void Ctor_Should_Throw_When_Price_Is_Not_Positive(double bad)
	{
		Action act = () => new InvoiceLine("desc", bad, 1);
		act.Should().Throw<ArgumentOutOfRangeException>()
			 .WithParameterName("price");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Ctor_Should_Throw_When_Quantity_Is_Not_Positive(int bad)
	{
		Action act = () => new InvoiceLine("desc", 1.0, bad);
		act.Should().Throw<ArgumentOutOfRangeException>()
			 .WithParameterName("quantity");
	}
}
