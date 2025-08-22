using DeliveryService.Domain.AggregatesModel;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public sealed class MockCarrierClient : ICarrierClient
	{
		public Task<(string carrier, string tracking)> BookLabelAsync(Shipment shipment)
		{
			// Simulate success with 95% probability
			if (Random.Shared.Next(0, 100) < 95)
				return Task.FromResult(("MockExpress", $"TRK-{shipment.Id.ToString()[..8]}"));
			throw new InvalidOperationException("Carrier booking failed (mock).");
		}
	}
}
