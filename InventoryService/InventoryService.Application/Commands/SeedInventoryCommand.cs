using MediatR;

namespace InventoryService.Application.Commands
{
	public record SeedInventoryCommand : IRequest<int>;
}

