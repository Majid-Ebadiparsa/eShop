namespace InvoiceService.Infrastructure.Messaging
{
	public class RabbitMqSettings
	{
		public string? CloudAmqpUrl { get; set; } = string.Empty;
		public string Host { get; set; } = string.Empty;
		public string VirtualHost { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}
}
