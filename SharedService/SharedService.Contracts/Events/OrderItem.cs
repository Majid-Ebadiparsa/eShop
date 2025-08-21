namespace SharedService.Contracts.Events
{
	public record OrderItem(Guid ProductId, int Quantity, string? Name = null);
}
