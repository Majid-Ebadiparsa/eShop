using FluentValidation;
using InvoiceService.Application.Invoices.Commands;

namespace InvoiceService.Application.Invoices.Validators
{
	public class SubmitInvoiceCommandValidator : AbstractValidator<SubmitInvoiceCommand>
	{
		public SubmitInvoiceCommandValidator()
		{
			RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
			RuleFor(x => x.Supplier).NotEmpty().MaximumLength(200);
			RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.AddDays(-1));
			RuleFor(x => x.Lines).NotEmpty();
			RuleForEach(x => x.Lines).ChildRules(lines =>
			{
				lines.RuleFor(l => l.Description).NotEmpty().MaximumLength(500);
				lines.RuleFor(l => l.Price).GreaterThan(0);
				lines.RuleFor(l => l.Quantity).GreaterThan(0);
			});
		}
	}
}
