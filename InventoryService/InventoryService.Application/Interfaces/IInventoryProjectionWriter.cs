namespace InventoryService.Application.Interfaces
{
    public interface IInventoryProjectionWriter
    {
        Task UpsertInventoryForOrderAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items, CancellationToken ct);
        Task ReleaseReservedInventoryAsync(Guid orderId, List<(Guid ProductId, int Quantity)> items, CancellationToken ct);
    }
}
