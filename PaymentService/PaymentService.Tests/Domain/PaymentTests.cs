using PaymentService.Domain.Aggregates;
using PaymentService.Domain.ValueObjects;
using Xunit;

namespace PaymentService.Tests.Domain
{
	public class PaymentTests
	{
		[Fact]
		public void Authorize_Then_Capture_Should_Set_Status_Captured()
		{
			var p = Payment.Create(Guid.NewGuid(), Money.From(10, "EUR"), PaymentMethod.Card("****1111"));
			p.MarkAuthorized("AUTH-1");
			p.MarkCaptured("CAP-1");
			Assert.Equal(PaymentStatus.Captured, p.Status);
		}
	}
}
