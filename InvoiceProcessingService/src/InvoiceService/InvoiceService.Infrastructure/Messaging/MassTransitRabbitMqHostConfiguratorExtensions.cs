using System;
using System.Security.Authentication;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace InvoiceService.Infrastructure.Messaging
{
	public static class MassTransitRabbitMqHostConfiguratorExtensions
	{
		public static void ConfigureRabbitMqHost(this IRabbitMqBusFactoryConfigurator cfg, IConfiguration configuration, RabbitMqSettings settings)
		{
			var cloudUrl = configuration["CLOUDAMQP_URL"];
			if (!string.IsNullOrWhiteSpace(cloudUrl))
			{
				ConfigureFromUri(cfg, cloudUrl);
				return;
			}

			if (!string.IsNullOrWhiteSpace(settings.CloudAmqpUrl))
			{
				ConfigureFromUri(cfg, settings.CloudAmqpUrl!);
				return;
			}

			var host = string.IsNullOrWhiteSpace(settings.Host) ? "localhost" : settings.Host;
			var vhost = string.IsNullOrWhiteSpace(settings.VirtualHost) ? "/" : settings.VirtualHost!;
			var user = string.IsNullOrWhiteSpace(settings.Username) ? "guest" : settings.Username!;
			var pass = string.IsNullOrWhiteSpace(settings.Password) ? "guest" : settings.Password!;

			cfg.Host(host, vhost, h =>
			{
				h.Username(user);
				h.Password(pass);
			});
		}

		private static void ConfigureFromUri(IRabbitMqBusFactoryConfigurator cfg, string amqpUrl)
		{
			var uri = new Uri(amqpUrl);

			var userInfo = Uri.UnescapeDataString(uri.UserInfo ?? string.Empty).Split(':', 2);
			var username = userInfo.Length > 0 ? userInfo[0] : "";
			var password = userInfo.Length > 1 ? userInfo[1] : "";

			var vhostPath = Uri.UnescapeDataString(uri.AbsolutePath ?? string.Empty).TrimStart('/');
			var vhost = string.IsNullOrEmpty(vhostPath) ? "/" : vhostPath;

			var host = uri.Host;
			var isAmqps = uri.Scheme.Equals("amqps", StringComparison.OrdinalIgnoreCase);
			var port = uri.IsDefaultPort
					? (isAmqps ? 5671 : 5672)
					: uri.Port;

			cfg.Host(host, (ushort)port, vhost, h =>
			{
				if (!string.IsNullOrEmpty(username)) h.Username(username);
				if (!string.IsNullOrEmpty(password)) h.Password(password);

				if (isAmqps)
				{
					h.UseSsl(s =>
					{
						s.Protocol = SslProtocols.Tls12;
					});
				}
			});
		}
	}
}
