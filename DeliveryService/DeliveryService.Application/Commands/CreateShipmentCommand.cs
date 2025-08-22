using DeliveryService.Application.DTOs;
using MediatR;

namespace DeliveryService.Application.Commands
{
	public record CreateShipmentCommand(
			Guid OrderId,
			AddressDto Address,
			IReadOnlyList<ShipmentItemDto> Items) : IRequest<Guid>;
}
