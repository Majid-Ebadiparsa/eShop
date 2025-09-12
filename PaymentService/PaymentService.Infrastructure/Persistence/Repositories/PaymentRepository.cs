using PaymentService.Application.Abstractions;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Infrastructure.Persistence.Repositories
{
	public class PaymentRepository : IPaymentRepository
	{
		private readonly PaymentDbContext _db;

		public PaymentRepository(PaymentDbContext db) => _db = db;

		public Task AddAsync(Payment payment, CancellationToken ct)
		{
			_db.Payments.Add(payment);
			return Task.CompletedTask;
		}
		
		public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
	}
}
