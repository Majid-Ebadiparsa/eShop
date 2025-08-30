namespace InvoiceSubscriber.Console.Options
{
	public class RabbitMqOptions
	{
		public string? CloudAmqpUrl { get; set; } = string.Empty;
		public string Host { get; set; } = "localhost";
		public string VirtualHost { get; set; } = "/";
		public string Username { get; set; } = "guest";
		public string Password { get; set; } = "guest";
		public string InvoiceSubmittedEndpointName { get; set; } = "invoice-submitted-console";
		public ushort PrefetchCount { get; set; } = 16;
		public int ConcurrentMessageLimit { get; set; } = 8;
	}
}
