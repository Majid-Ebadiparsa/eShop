namespace OrderService.Shared.DTOs
{
	public record OrderItemDto(
		Guid ProductId,
		int Quantity,
		decimal UnitPrice
	);
}
