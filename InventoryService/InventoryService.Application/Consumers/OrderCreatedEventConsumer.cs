using InventoryService.Application.Events;
using InventoryService.Application.Interfaces;
using MassTransit;

namespace InventoryService.Application.Consumers
{
	public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
	{
		private readonly IInventoryRepository _inventoryRepository;

		public OrderCreatedEventConsumer(IInventoryRepository repository)
		{
			_inventoryRepository = repository;
		}

		public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
		{
			foreach (var item in context.Message.Items)
			{
				var inventory = await _inventoryRepository.GetByProductIdAsync(item.ProductId, context.CancellationToken);

				if (inventory is not null)
				{
					try
					{
						inventory.Decrease(item.Quantity);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Inventory error: {ex.Message}");
					}
				}
			}

			await _inventoryRepository.SaveChangesAsync(context.CancellationToken);
		}
	}
}
