using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Domain.ValueObjects
{
	public sealed record Money(decimal Amount, string Currency)
	{
		public static Money From(decimal amount, string currency)
				=> new(amount < 0 ? throw new ArgumentException("Amount < 0") : amount,
							 string.IsNullOrWhiteSpace(currency) ? "EUR" : currency.Trim().ToUpperInvariant());		

		public Money Add(Money other)
			=> Currency != other.Currency
					? throw new InvalidOperationException("Cannot add amounts with different currencies.")
					: new Money(Amount + other.Amount, Currency);
	}
}
