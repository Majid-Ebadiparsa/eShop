using InventoryService.Application.Interfaces;
using InventoryService.Domain.AggregatesModel;
using InventoryService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories
{
	public class InventoryRepository : IInventoryRepository
	{
		private readonly InventoryDbContext _context;

		public InventoryRepository(InventoryDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(InventoryItem item)
		{
			await _context.InventoryItems.AddAsync(item);
		}

		public async Task<InventoryItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken)
		{
			return await _context.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
		}

		public async Task SaveChangesAsync(CancellationToken cancellationToken)
		{
			await _context.SaveChangesAsync(cancellationToken);
		}
	}
}
