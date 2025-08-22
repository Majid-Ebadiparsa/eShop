using MediatR;

namespace DeliveryService.Application.Queries
{
	public record GetShipmentByIdQuery(Guid ShipmentId) : IRequest<ShipmentReadModel?>;
}
