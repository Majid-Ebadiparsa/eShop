using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Aggregates
{
	public class Payment
	{
		public Guid Id { get; private set; }
		public Guid OrderId { get; private set; }
		public Money Amount { get; private set; }
		public PaymentMethod Method { get; private set; }
		public PaymentStatus Status { get; private set; }
		private readonly List<PaymentAttempt> _attempts = new();
		public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();

		private Payment() { } // EF
		private Payment(Guid orderId, Money amount, PaymentMethod method)
		{
			Id = Guid.NewGuid();
			OrderId = orderId;
			Amount = amount;
			Method = method;
			Status = PaymentStatus.Initiated;
		}

		public static Payment Create(Guid orderId, Money amount, PaymentMethod method)
				=> new(orderId, amount, method);

		public void MarkAuthorized(string authCode)
		{
			if (Status is PaymentStatus.Captured or PaymentStatus.Refunded)
				throw new InvalidOperationException("Already finalized.");
			Status = PaymentStatus.Authorized;
			_attempts.Add(PaymentAttempt.Success("AUTHORIZE", authCode));
		}

		public void MarkCaptured(string captureId)
		{
			if (Status != PaymentStatus.Authorized)
				throw new InvalidOperationException("Capture requires Authorized.");
			Status = PaymentStatus.Captured;
			_attempts.Add(PaymentAttempt.Success("CAPTURE", captureId));
		}

		public void MarkFailed(string operation, string reason)
		{
			Status = PaymentStatus.Failed;
			_attempts.Add(PaymentAttempt.Fail(operation, reason));
		}

		public void MarkRefunded(string refundId)
		{
			if (Status != PaymentStatus.Captured)
				throw new InvalidOperationException("Refund requires Captured.");
			Status = PaymentStatus.Refunded;
			_attempts.Add(PaymentAttempt.Success("REFUND", refundId));
		}

		public void Cancel(string reason)
		{
			if (Status is PaymentStatus.Captured or PaymentStatus.Refunded)
				throw new InvalidOperationException("Cannot cancel finalized payment.");
			Status = PaymentStatus.Cancelled;
			_attempts.Add(PaymentAttempt.Fail("CANCEL", reason));
		}

		public void Refund(string refundId)
		{
			if (Status != PaymentStatus.Captured)
				throw new InvalidOperationException("Only captured payments can be refunded.");
			Status = PaymentStatus.Refunded;
			_attempts.Add(PaymentAttempt.Success("REFUND", refundId));
		}
	}
}
