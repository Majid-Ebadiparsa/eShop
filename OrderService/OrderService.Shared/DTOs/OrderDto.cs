namespace OrderService.Shared.DTOs
{
	public record OrderDto(
		Guid Id,
		Guid CustomerId,
		string Street,
		string City,
		string PostalCode,
		DateTime OrderDate,
		decimal TotalAmount,
		List<OrderItemDto> Items
	);
}
