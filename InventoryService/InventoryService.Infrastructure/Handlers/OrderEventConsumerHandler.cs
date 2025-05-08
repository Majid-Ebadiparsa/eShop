using InventoryService.Application.Events;
using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Repositories.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryService.Infrastructure.Handlers
{
	public class OrderEventConsumerHandler : IOrderEventConsumer
	{
		private readonly InventoryDbContext _context;
		private readonly ILogger<OrderEventConsumerHandler> _logger;

		public OrderEventConsumerHandler(InventoryDbContext context, ILogger<OrderEventConsumerHandler> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
		{
			foreach (var item in @event.Items)
			{
				var inventory = await _context.InventoryItems
					.FirstOrDefaultAsync(i => i.ProductId == item.ProductId, cancellationToken);

				if (inventory is not null)
				{
					inventory.Decrease(item.Quantity);
				}
				else
				{
					_logger.LogWarning("Inventory not found for ProductId {ProductId}", item.ProductId);
				}
			}

			await _context.SaveChangesAsync(cancellationToken);
		}
	}
}
