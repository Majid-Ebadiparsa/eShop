using InventoryService.Domain.AggregatesModel;
using MediatR;

namespace InventoryService.Application.Queries
{
	public record GetInventoryByProductIdQuery(Guid ProductId) : IRequest<InventoryItem?>;
}
