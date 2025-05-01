using MediatR;
using OrderService.Shared.DTOs;

namespace OrderService.Application.Queries
{
	public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
}
