using InventoryService.Application.Interfaces;
using InventoryService.Domain.AggregatesModel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryService.Application.Commands
{
	public class SeedInventoryCommandHandler : IRequestHandler<SeedInventoryCommand, int>
	{
		private readonly IInventoryRepository _repository;
		private readonly ILogger<SeedInventoryCommandHandler> _logger;

		public SeedInventoryCommandHandler(
			IInventoryRepository repository,
			ILogger<SeedInventoryCommandHandler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<int> Handle(SeedInventoryCommand request, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting inventory seed data creation...");

			var sampleProducts = GetSampleProducts();
			int seededCount = 0;

			foreach (var (productId, productName, quantity) in sampleProducts)
			{
				// Check if product already exists
				var existing = await _repository.GetByProductIdAsync(productId, cancellationToken);
				if (existing != null)
				{
					_logger.LogInformation("Product {ProductId} ({ProductName}) already exists, skipping", productId, productName);
					continue;
				}

				// Create new inventory item
				var inventoryItem = new InventoryItem(Guid.NewGuid(), productId, quantity);
				await _repository.AddAsync(inventoryItem);
				seededCount++;

				_logger.LogInformation("Seeded product {ProductId} ({ProductName}) with {Quantity} units", 
					productId, productName, quantity);
			}

			await _repository.SaveChangesAsync(cancellationToken);

			_logger.LogInformation("Inventory seed completed. {Count} products seeded", seededCount);
			return seededCount;
		}

		private static List<(Guid ProductId, string ProductName, int Quantity)> GetSampleProducts()
		{
			return new List<(Guid, string, int)>
			{
				// Electronics
				(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Laptop - Dell XPS 15", 50),
				(Guid.Parse("11111111-1111-1111-1111-111111111112"), "Laptop - MacBook Pro 16\"", 30),
				(Guid.Parse("11111111-1111-1111-1111-111111111113"), "Desktop PC - Gaming Rig", 25),
				(Guid.Parse("11111111-1111-1111-1111-111111111114"), "Monitor - 27\" 4K", 100),
				(Guid.Parse("11111111-1111-1111-1111-111111111115"), "Keyboard - Mechanical RGB", 150),
				(Guid.Parse("11111111-1111-1111-1111-111111111116"), "Mouse - Wireless Gaming", 200),
				(Guid.Parse("11111111-1111-1111-1111-111111111117"), "Headset - Noise Cancelling", 75),
				(Guid.Parse("11111111-1111-1111-1111-111111111118"), "Webcam - 1080p HD", 80),

				// Mobile Devices
				(Guid.Parse("22222222-2222-2222-2222-222222222221"), "Smartphone - iPhone 15 Pro", 100),
				(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Smartphone - Samsung Galaxy S24", 120),
				(Guid.Parse("22222222-2222-2222-2222-222222222223"), "Tablet - iPad Air", 60),
				(Guid.Parse("22222222-2222-2222-2222-222222222224"), "Tablet - Surface Pro", 40),
				(Guid.Parse("22222222-2222-2222-2222-222222222225"), "Smartwatch - Apple Watch", 90),
				(Guid.Parse("22222222-2222-2222-2222-222222222226"), "Smartwatch - Samsung Galaxy Watch", 85),

				// Audio Equipment
				(Guid.Parse("33333333-3333-3333-3333-333333333331"), "Speakers - Bluetooth Portable", 150),
				(Guid.Parse("33333333-3333-3333-3333-333333333332"), "Speakers - Home Theater System", 40),
				(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Earbuds - Wireless ANC", 200),
				(Guid.Parse("33333333-3333-3333-3333-333333333334"), "Microphone - USB Condenser", 70),

				// Accessories
				(Guid.Parse("44444444-4444-4444-4444-444444444441"), "USB-C Hub - 7-in-1", 180),
				(Guid.Parse("44444444-4444-4444-4444-444444444442"), "External SSD - 1TB", 95),
				(Guid.Parse("44444444-4444-4444-4444-444444444443"), "Power Bank - 20000mAh", 130),
				(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Laptop Bag - Professional", 110),
				(Guid.Parse("44444444-4444-4444-4444-444444444445"), "Phone Case - Protective", 300),
				(Guid.Parse("44444444-4444-4444-4444-444444444446"), "Screen Protector - Tempered Glass", 400),
				(Guid.Parse("44444444-4444-4444-4444-444444444447"), "Charging Cable - USB-C 2m", 500),

				// Home & Office
				(Guid.Parse("55555555-5555-5555-5555-555555555551"), "Desk Lamp - LED Adjustable", 85),
				(Guid.Parse("55555555-5555-5555-5555-555555555552"), "Ergonomic Chair - Office", 45),
				(Guid.Parse("55555555-5555-5555-5555-555555555553"), "Standing Desk - Electric", 30),
				(Guid.Parse("55555555-5555-5555-5555-555555555554"), "Monitor Arm - Dual Mount", 65),
				(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Cable Management Kit", 120)
			};
		}
	}
}

