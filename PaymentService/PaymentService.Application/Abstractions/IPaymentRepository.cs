using PaymentService.Domain.Aggregates;

namespace PaymentService.Application.Abstractions
{
	public interface IPaymentRepository
	{
		Task AddAsync(Payment payment, CancellationToken ct);

		Task SaveChangesAsync(CancellationToken ct);
	}
}
