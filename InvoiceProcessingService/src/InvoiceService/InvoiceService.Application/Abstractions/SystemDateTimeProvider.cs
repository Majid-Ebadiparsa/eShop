namespace InvoiceService.Application.Abstractions
{
	public sealed class SystemDateTimeProvider : IDateTimeProvider
	{
		public DateTime UtcNow => DateTime.UtcNow;
	}
}
