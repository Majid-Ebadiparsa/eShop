using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Configuration;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Projections;
using OrderService.Infrastructure.Repositories;
using OrderService.Infrastructure.Repositories.EF;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace OrderService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddTransient(typeof(IOrderRepository), typeof(OrderRepository))
                .AddScoped<IOrderEventConsumer, OrderEventConsumerHandler>()
                .RegisterDbContext(configuration)
                .RegisterMassTransit(configuration)
                .AddHealthChecks(configuration);

            return services;
        }

        private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            //EF: SQLServer
            services.AddDbContext<OrderDbContext>(options =>
                 options.UseSqlServer(
                        configuration.GetConnectionString("OrderDb"),
                        b => b.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName)));

            //EF: InMemory
            //services.AddDbContext<OrderDbContext>(options =>
            //	 options.UseInMemoryDatabase(databaseName: "OrderDb"), ServiceLifetime.Scoped, ServiceLifetime.Scoped);

            return services;
        }

        private static IServiceCollection RegisterMassTransit(this IServiceCollection services, IConfiguration configuration/*, Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configure*/)
        {
            // Bind RabbitMQ settings
            var rabbitMqSettings = new RabbitMqSettings();
            configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderProjectionConsumer>();

                x.SetKebabCaseEndpointNameFormatter();
                x.AddConsumer<InventoryReservedConsumer>();
                x.AddConsumer<InventoryReservationFailedConsumer>();
                x.AddConsumer<PaymentAuthorizedConsumer>();
                x.AddConsumer<PaymentCapturedConsumer>();
                x.AddConsumer<PaymentFailedConsumer>();
                x.AddConsumer<ShipmentCreatedConsumer>();
                x.AddConsumer<ShipmentDispatchedConsumer>();
                x.AddConsumer<ShipmentDeliveredConsumer>();

                x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
                {
                    o.UseSqlServer();
                    o.QueryDelay = TimeSpan.FromSeconds(1);
                    o.DuplicateDetectionWindow = TimeSpan.FromMinutes(1);
                });

                x.UsingRabbitMq((context, cfgBus) =>
                {
                    cfgBus.Host(rabbitMqSettings.Host, rabbitMqSettings.VirtualHost, h =>
                    {
                        h.Username(rabbitMqSettings.Username);
                        h.Password(rabbitMqSettings.Password);
                    });

                    if (!string.IsNullOrWhiteSpace(rabbitMqSettings.ReceiveEndpoint))
                    {
                        cfgBus.ReceiveEndpoint(rabbitMqSettings.ReceiveEndpoint, e =>
                        {
                            e.ConfigureConsumer<OrderProjectionConsumer>(context);
                            e.ConfigureConsumer<InventoryReservedConsumer>(context);
                            e.ConfigureConsumer<InventoryReservationFailedConsumer>(context);
                            e.ConfigureConsumer<PaymentAuthorizedConsumer>(context);
                            e.ConfigureConsumer<PaymentCapturedConsumer>(context);
                            e.ConfigureConsumer<PaymentFailedConsumer>(context);
                            e.ConfigureConsumer<ShipmentCreatedConsumer>(context);
                            e.ConfigureConsumer<ShipmentDispatchedConsumer>(context);
                            e.ConfigureConsumer<ShipmentDeliveredConsumer>(context);
                        });
                    }

                    // Activate MassTransit endpoints and Outbox integration
                    cfgBus.ConfigureEndpoints(context);
                });
            });

            services.AddScoped<IEventPublisher, EfOutboxEventPublisher>();

            // MongoDB read model projection
            services.AddSingleton<IMongoClient>(_ => new MongoClient(configuration["Mongo:Connection"] ?? configuration.GetConnectionString("Mongo")));
            services.AddScoped<IOrderProjectionWriter, MongoOrderProjectionWriter>();


            return services;
        }

        private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqSettings = new RabbitMqSettings();
            configuration.GetSection("RabbitMq").Bind(rabbitMqSettings);

            var rabbitMqConn = $"amqp://{rabbitMqSettings.Username}:{rabbitMqSettings.Password}@{rabbitMqSettings.Host}:5672{rabbitMqSettings.VirtualHost}";

            services
                .AddHealthChecks()
                // Redis (if configured) - use a lightweight DI-resolved health check to avoid adding an extra package
                .AddCheck<RedisHealthCheck>(name: "redis", tags: new[] { "ready" })
                .AddDbContextCheck<OrderDbContext>(name: "sqlserver")
                .AddRabbitMQ(sp =>
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(rabbitMqConn),
                        AutomaticRecoveryEnabled = true
                    };
                    return factory.CreateConnectionAsync("healthcheck");
                }, name: "rabbitmq", tags: new[] { "ready" });

            return services;
        }
    }

    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        public RedisHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var conn = _configuration["Redis:Connection"] ?? "redis:6379";
            try
            {
                var mux = await ConnectionMultiplexer.ConnectAsync(conn);
                if (mux.IsConnected)
                {
                    await mux.CloseAsync();
                    return HealthCheckResult.Healthy();
                }
                await mux.CloseAsync();
                return HealthCheckResult.Unhealthy("Unable to connect to Redis");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(exception: ex);
            }
        }
    }
}
