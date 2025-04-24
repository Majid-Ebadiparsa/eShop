using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Commands
{
	public record PlaceOrderCommand(
		Guid CustomerId,
		string Street,
		string City,
		string PostalCode,
		List<PlaceOrderItemDto> Items) : IRequest<Guid>;
}
