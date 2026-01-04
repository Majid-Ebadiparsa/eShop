using InventoryService.Shared.DTOs;
using MediatR;

namespace InventoryService.Application.Queries
{
	public record GetInventoryByProductIdQuery(Guid ProductId) : IRequest<InventoryDto?>;
}
