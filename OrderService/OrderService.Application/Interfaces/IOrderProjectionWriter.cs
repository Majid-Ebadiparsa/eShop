namespace OrderService.Application.Interfaces
{
    public interface IOrderProjectionWriter
    {
        Task UpsertOrderAsync(Guid orderId, List<(Guid ProductId, int Quantity, decimal UnitPrice)> items, CancellationToken ct);
    }
}
