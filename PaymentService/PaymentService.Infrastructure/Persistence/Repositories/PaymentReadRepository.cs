using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Abstractions;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Infrastructure.Persistence.Repositories
{
	public class PaymentReadRepository : IPaymentReadRepository
	{
		private readonly PaymentDbReaderContext _db;

		public PaymentReadRepository(PaymentDbReaderContext db) => _db = db;


		public async ValueTask<Payment?> FindAsync(Guid paymentId, CancellationToken ct)
		{
			return await _db.Payments.FindAsync(new object[] { paymentId }, ct);
		}

		public async Task<Payment?> FirstOrDefaultAsync(Guid paymentId, CancellationToken ct)
		{
			return await _db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId, ct);
		}
	}
}
