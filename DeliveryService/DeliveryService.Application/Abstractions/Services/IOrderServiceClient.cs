using DeliveryService.Application.DTOs;

namespace DeliveryService.Application.Abstractions.Services
{
	public interface IOrderServiceClient
	{
		Task<OrderDetailsDto?> GetOrderDetailsAsync(Guid orderId, CancellationToken cancellationToken);
	}

	public record OrderDetailsDto(
		Guid OrderId,
		Guid CustomerId,
		string Street,
		string City,
		string PostalCode,
		List<OrderItemDetailsDto> Items
	);

	public record OrderItemDetailsDto(Guid ProductId, int Quantity);
}

