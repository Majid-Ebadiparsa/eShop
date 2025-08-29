using InvoiceSubscriber.Console.Consumers;
using InvoiceSubscriber.Console.Inbox;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InvoiceSubscriber.Console.Extensions
{
	public static class DependencyInjection
	{
		const string SECTION_NAME = "InboxDb";

		public static IServiceCollection AddInboxStore(
				this IServiceCollection services,
				IConfiguration configuration,
				IHostEnvironment env,
				Action<SqliteInboxOptions>? configure = null)
		{
			services.AddSingleton<IInboxStore>(sp =>
			{
				var cs = GetConnectionString(configuration, env);

				var options = new SqliteInboxOptions(cs)
				{
					BusyTimeout = TimeSpan.FromSeconds(5),
					JournalMode = "WAL"
				};

				configure?.Invoke(options);
				return new SqliteInboxStore(options);
			});

			return services;
		}

		public static IServiceCollection AddSubscriberMessaging(
				this IServiceCollection services,
				IConfiguration configuration)
		{
			services.AddMassTransit(x =>
			{
				x.AddConsumer<InvoiceSubmittedConsumer>();

				x.UsingRabbitMq((ctx, cfg) =>
				{
					var host = configuration["RabbitMQ:Host"] ?? "localhost";
					var vhost = configuration["RabbitMQ:VirtualHost"] ?? "/";
					var user = configuration["RabbitMQ:Username"] ?? "guest";
					var pass = configuration["RabbitMQ:Password"] ?? "guest";

					cfg.Host(host, vhost, h =>
					{
						h.Username(user);
						h.Password(pass);
					});

					cfg.ReceiveEndpoint("invoice-submitted-console", e =>
					{
						e.PrefetchCount = 16;              // Consumption optimization
						e.ConcurrentMessageLimit = 8;      // Concurrency control
						e.UseMessageRetry(r => r.Exponential(
								retryLimit: 5,
								minInterval: TimeSpan.FromSeconds(1),
								maxInterval: TimeSpan.FromSeconds(30),
								intervalDelta: TimeSpan.FromSeconds(5)));
						e.ConfigureConsumer<InvoiceSubmittedConsumer>(ctx);
					});
				});
			});

			return services;
		}

		private static string GetConnectionString(IConfiguration cfg, IHostEnvironment env)
		{
			var raw = cfg.GetConnectionString(SECTION_NAME) ?? "Data Source=../data/inbox.db";
			var csb = new SqliteConnectionStringBuilder(raw);
			var dataSource = csb.DataSource;

			if (Path.IsPathRooted(dataSource))
			{
				EnsureDirectoryFor(dataSource);
				return csb.ToString();
			}

			// Detect if running in a container
			bool inContainer = IsRunningInContainer();

			string rootedPath = GetRootedPath(env, dataSource, inContainer);
			EnsureDirectoryFor(rootedPath);
			csb.DataSource = rootedPath;

			if (!csb.ContainsKey("Mode")) csb.Mode = SqliteOpenMode.ReadWriteCreate;
			if (!csb.ContainsKey("Cache")) csb.Cache = SqliteCacheMode.Shared;
			if (!csb.ContainsKey("Pooling")) csb.Pooling = true;

			return csb.ToString();





			static void EnsureDirectoryFor(string filePath)
			{
				var dir = Path.GetDirectoryName(filePath);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
					Directory.CreateDirectory(dir);
			}

			static string? FindProjectRoot(string start)
			{
				var dir = new DirectoryInfo(start);
				while (dir != null)
				{
					if (dir.EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly).Any())
						return dir.FullName;

					dir = dir.Parent;
				}
				return null;
			}

			static string GetRootedPath(IHostEnvironment env, string dataSource, bool inContainer)
			{
				string rootedPath;
				if (inContainer)
				{
					rootedPath = Path.GetFullPath(Path.Combine("/app/data", dataSource.TrimStart('.', '/', '\\')));
				}
				else
				{
					var projectRoot = FindProjectRoot(env.ContentRootPath) ?? env.ContentRootPath;
					rootedPath = Path.GetFullPath(Path.Combine(projectRoot, dataSource));
				}

				return rootedPath;
			}

			static bool IsRunningInContainer()
			{
				return string.Equals(
						Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
						"true",
						StringComparison.OrdinalIgnoreCase);
			}
		}
	}
}
