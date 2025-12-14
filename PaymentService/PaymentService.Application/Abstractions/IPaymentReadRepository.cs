using PaymentService.Domain.Aggregates;

namespace PaymentService.Application.Abstractions
{
	public interface IPaymentReadRepository
	{
		ValueTask<Payment?> FindAsync(Guid paymentId, CancellationToken ct);
		Task<Payment?> FirstOrDefaultAsync(Guid paymentId, CancellationToken ct);
	}
}
