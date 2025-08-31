namespace InvoiceService.Application.Abstractions
{
	public interface IDateTimeProvider
	{
		DateTime UtcNow { get; }
	}
}
