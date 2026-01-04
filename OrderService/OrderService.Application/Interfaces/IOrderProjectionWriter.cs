namespace OrderService.Application.Interfaces
{
    public interface IOrderProjectionWriter
    {
        Task UpsertOrderAsync(Guid orderId, Guid customerId, string street, string city, string postalCode, List<(Guid ProductId, int Quantity, decimal UnitPrice)> items, CancellationToken ct);
        Task UpdateStatusAsync(Guid orderId, string status, CancellationToken ct);
    }
}
