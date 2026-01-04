using PaymentService.Domain.Aggregates;
using PaymentService.Domain.ValueObjects;
using Xunit;

namespace PaymentService.Tests.Domain
{
	public class PaymentTests
	{
		[Fact]
		public void Create_Payment_Should_Initialize_With_Pending_Status()
		{
			// Arrange
			var orderId = Guid.NewGuid();
			var amount = Money.From(100.50m, "USD");
			var paymentMethod = PaymentMethod.Card("****1234");

			// Act
			var payment = Payment.Create(orderId, amount, paymentMethod);

			// Assert
			Assert.Equal(orderId, payment.OrderId);
			Assert.Equal(PaymentStatus.Pending, payment.Status);
			Assert.Equal(amount, payment.Amount);
			Assert.Equal(paymentMethod, payment.PaymentMethodUsed);
		}

		[Fact]
		public void Authorize_Then_Capture_Should_Set_Status_Captured()
		{
			// Arrange
			var payment = Payment.Create(Guid.NewGuid(), Money.From(10, "EUR"), PaymentMethod.Card("****1111"));

			// Act
			payment.MarkAuthorized("AUTH-1");
			payment.MarkCaptured("CAP-1");

			// Assert
			Assert.Equal(PaymentStatus.Captured, payment.Status);
		}

		[Fact]
		public void MarkAuthorized_Should_Update_Status_And_Set_AuthorizationId()
		{
			// Arrange
			var payment = Payment.Create(Guid.NewGuid(), Money.From(50, "USD"), PaymentMethod.Card("****5555"));
			var authId = "AUTH-12345";

			// Act
			payment.MarkAuthorized(authId);

			// Assert
			Assert.Equal(PaymentStatus.Authorized, payment.Status);
		}

		[Fact]
		public void MarkFailed_Should_Set_Status_Failed()
		{
			// Arrange
			var payment = Payment.Create(Guid.NewGuid(), Money.From(25, "GBP"), PaymentMethod.Card("****9999"));
			var errorMessage = "Insufficient funds";

			// Act
			payment.MarkFailed(errorMessage);

			// Assert
			Assert.Equal(PaymentStatus.Failed, payment.Status);
		}

		[Fact]
		public void MarkRefunded_Should_Set_Status_Refunded()
		{
			// Arrange
			var payment = Payment.Create(Guid.NewGuid(), Money.From(75, "EUR"), PaymentMethod.Card("****2222"));
			payment.MarkAuthorized("AUTH-1");
			payment.MarkCaptured("CAP-1");
			var refundId = "REF-123";

			// Act
			payment.MarkRefunded(refundId);

			// Assert
			Assert.Equal(PaymentStatus.Refunded, payment.Status);
		}

		[Fact]
		public void MarkCancelled_Should_Set_Status_Cancelled()
		{
			// Arrange
			var payment = Payment.Create(Guid.NewGuid(), Money.From(40, "USD"), PaymentMethod.Card("****3333"));
			payment.MarkAuthorized("AUTH-1");

			// Act
			payment.MarkCancelled();

			// Assert
			Assert.Equal(PaymentStatus.Cancelled, payment.Status);
		}

		[Fact]
		public void Money_With_Same_Amount_And_Currency_Should_Be_Equal()
		{
			// Arrange
			var money1 = Money.From(100, "USD");
			var money2 = Money.From(100, "USD");

			// Act & Assert
			Assert.Equal(money1, money2);
		}

		[Fact]
		public void Money_With_Different_Currency_Should_Not_Be_Equal()
		{
			// Arrange
			var money1 = Money.From(100, "USD");
			var money2 = Money.From(100, "EUR");

			// Act & Assert
			Assert.NotEqual(money1, money2);
		}

		[Fact]
		public void Money_Addition_Should_Work_For_Same_Currency()
		{
			// Arrange
			var money1 = Money.From(50, "USD");
			var money2 = Money.From(30, "USD");

			// Act
			var result = money1.Add(money2);

			// Assert
			Assert.Equal(80, result.Amount);
			Assert.Equal("USD", result.Currency);
		}

		[Fact]
		public void Money_Addition_With_Different_Currencies_Should_Throw_Exception()
		{
			// Arrange
			var money1 = Money.From(50, "USD");
			var money2 = Money.From(30, "EUR");

			// Act & Assert
			Assert.Throws<InvalidOperationException>(() => money1.Add(money2));
		}

		[Fact]
		public void PaymentMethod_Card_Should_Create_Card_Type()
		{
			// Arrange & Act
			var paymentMethod = PaymentMethod.Card("****1234");

			// Assert
			Assert.Equal(PaymentMethodType.CreditCard, paymentMethod.Type);
			Assert.Equal("****1234", paymentMethod.Last4Digits);
		}

		[Fact]
		public void PaymentMethod_BankTransfer_Should_Create_BankTransfer_Type()
		{
			// Arrange & Act
			var paymentMethod = PaymentMethod.BankTransfer("IBAN1234");

			// Assert
			Assert.Equal(PaymentMethodType.BankTransfer, paymentMethod.Type);
		}

		[Fact]
		public void Payment_Should_Track_Attempts()
		{
			// Arrange
			var payment = Payment.Create(Guid.NewGuid(), Money.From(100, "USD"), PaymentMethod.Card("****1111"));

			// Act
			payment.MarkAuthorized("AUTH-1");

			// Assert
			Assert.NotEmpty(payment.Attempts);
			Assert.Single(payment.Attempts);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-10)]
		[InlineData(-100.5)]
		public void Money_With_Zero_Or_Negative_Amount_Should_Throw_Exception(decimal amount)
		{
			// Act & Assert
			Assert.Throws<ArgumentException>(() => Money.From(amount, "USD"));
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void Money_With_Empty_Currency_Should_Throw_Exception(string currency)
		{
			// Act & Assert
			Assert.Throws<ArgumentException>(() => Money.From(100, currency));
		}
	}
}
