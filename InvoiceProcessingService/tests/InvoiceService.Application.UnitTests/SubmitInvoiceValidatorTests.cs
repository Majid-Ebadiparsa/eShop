using FluentAssertions;
using FluentValidation.TestHelper;
using InvoiceService.Application.Invoices.Commands;
using InvoiceService.Application.Invoices.Validators;

namespace InvoiceService.Application.UnitTests
{
	public class SubmitInvoiceValidatorTests
	{
		private readonly SubmitInvoiceCommandValidator _validator = new();

		[Fact]
		public void Invalid_When_No_Lines()
		{
			var cmd = new SubmitInvoiceCommand("d", DateTime.UtcNow.AddDays(1), "ACME", new());
			_validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Lines);
		}

		[Fact]
		public void Invalid_When_Past_DueDate()
		{
			var cmd = new SubmitInvoiceCommand("d", DateTime.UtcNow.AddDays(-1), "ACME",
					new() { new SubmitInvoiceLine("A", 1, 1) });
			_validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.DueDate);
		}

		[Fact]
		public void Valid_Command()
		{
			var cmd = new SubmitInvoiceCommand("d", DateTime.UtcNow.AddDays(2), "ACME",
					new() { new SubmitInvoiceLine("A", 1.2, 3) });
			_validator.TestValidate(cmd).IsValid.Should().BeTrue();
		}
	}
}
