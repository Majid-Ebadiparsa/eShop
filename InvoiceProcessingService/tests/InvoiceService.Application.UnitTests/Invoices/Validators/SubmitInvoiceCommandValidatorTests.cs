using InvoiceService.Application.Invoices.Commands;
using InvoiceService.Application.Invoices.Validators;
using InvoiceService.Application.UnitTests.Builders;
using FluentValidation.TestHelper;

namespace InvoiceService.Application.UnitTests.Invoices.Validators
{
	public class SubmitInvoiceCommandValidatorTests
	{
		private readonly SubmitInvoiceCommandValidator _sut = new();

		[Fact]
		public void Valid_Command_Should_Pass()
		{
			var cmd = new SubmitInvoiceCommandBuilder().Build();
			var result = _sut.TestValidate(cmd);
			result.ShouldNotHaveAnyValidationErrors();
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public void Description_Should_Not_Be_Empty(string? description)
		{
			var cmd = new SubmitInvoiceCommandBuilder().WithDescription(description ?? string.Empty).Build();
			var result = _sut.TestValidate(cmd);
			result.ShouldHaveValidationErrorFor(x => x.Description);
		}

		[Fact]
		public void Description_Should_Not_Exceed_500()
		{
			var longText = new string('x', 501);
			var cmd = new SubmitInvoiceCommandBuilder().WithDescription(longText).Build();
			var result = _sut.TestValidate(cmd);
			result.ShouldHaveValidationErrorFor(x => x.Description);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("   ")]
		public void Supplier_Should_Not_Be_Empty(string? supplier)
		{
			var cmd = new SubmitInvoiceCommandBuilder().WithSupplier(supplier ?? string.Empty).Build();
			var result = _sut.TestValidate(cmd);
			result.ShouldHaveValidationErrorFor(x => x.Supplier);
		}

		[Fact]
		public void Supplier_Should_Not_Exceed_200()
		{
			var longText = new string('x', 201);
			var cmd = new SubmitInvoiceCommandBuilder().WithSupplier(longText).Build();
			var result = _sut.TestValidate(cmd);
			result.ShouldHaveValidationErrorFor(x => x.Supplier);
		}

		[Fact]
		public void DueDate_Should_Be_In_Future_Or_Today()
		{
			// Validator rule: GreaterThan(DateTime.UtcNow.AddDays(-1))
			var yesterdayUtc = DateTime.UtcNow.AddDays(-2);
			var cmd = new SubmitInvoiceCommandBuilder().WithDueDate(yesterdayUtc).Build();
			var result = _sut.TestValidate(cmd);
			result.ShouldHaveValidationErrorFor(x => x.DueDate);
		}

		[Fact]
		public void Lines_Should_Not_Be_Empty()
		{
			var cmd = new SubmitInvoiceCommandBuilder().WithLines(Array.Empty<SubmitInvoiceLine>()).Build();
			var result = _sut.TestValidate(cmd);
			result.ShouldHaveValidationErrorFor(x => x.Lines);
		}

		[Fact]
		public void Line_Price_And_Quantity_Must_Be_Positive()
		{
			var cmd = new SubmitInvoiceCommandBuilder()
					.WithLines(
							new SubmitInvoiceLine("Paper", 0, 1),    // invalid price
							new SubmitInvoiceLine("Pen", 1.2, 0))    // invalid qty
					.Build();

			var result = _sut.TestValidate(cmd);
			result.ShouldHaveValidationErrorFor("Lines[0].Price");
			result.ShouldHaveValidationErrorFor("Lines[1].Quantity");
		}
	}
}
