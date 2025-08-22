using DeliveryService.Domain.AggregatesModel;

namespace DeliveryService.Infrastructure.Messaging.Consumers
{
	public interface ICarrierClient
	{
		Task<(string carrier, string tracking)> BookLabelAsync(Shipment shipment);
	}
}
