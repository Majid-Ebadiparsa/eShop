namespace DeliveryService.Application.Queries
{
	public record ShipmentReadModel(
			Guid ShipmentId, Guid OrderId, string Status, string? Carrier, string? TrackingNumber,
			string Street, string City, string Zip, string Country, DateTime CreatedAtUtc);
}
