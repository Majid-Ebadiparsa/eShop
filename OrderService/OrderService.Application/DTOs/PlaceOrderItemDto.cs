namespace OrderService.Application.DTOs
{
	public record PlaceOrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);
}
