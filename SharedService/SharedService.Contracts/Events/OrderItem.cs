namespace SharedService.Contracts.Events
{
	public record OrderItem(Guid ProductId, int Quantity, decimal UnitPrice, string? Name = null);
}
