using System;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Shared.Contracts.Events;
using InvoiceSubscriber.Console.Abstractions;
using InvoiceSubscriber.Console.Messaging.Consumers;

namespace InvoiceSubscriber.ConsumerTests
{
	public class InvoiceSubmittedConsumerTests
	{
		[Fact]
		public async Task Should_Consume_InvoiceSubmitted_Message()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IInboxStore, InMemoryInboxStore>();

			services.AddMassTransitInMemoryTestHarness(cfg =>
			{
				cfg.AddConsumer<InvoiceSubmittedConsumer>();
				cfg.AddConsumerTestHarness<InvoiceSubmittedConsumer>();
			});

			using var provider = services.BuildServiceProvider(validateScopes: true);

			var harness = provider.GetRequiredService<InMemoryTestHarness>();
			var consumerHarness = provider.GetRequiredService<IConsumerTestHarness<InvoiceSubmittedConsumer>>();

			await harness.Start();
			try
			{
				var msg = new InvoiceSubmitted
				{
					InvoiceId = Guid.NewGuid(),
					Description = "Office supplies",
					DueDate = DateTime.UtcNow.AddDays(3),
					Supplier = "ACME GmbH",
					Lines = new[]
						{
								new InvoiceLineItem{Description = "Pens", Price = 1.2, Quantity = 10 },
								new InvoiceLineItem{Description = "Notebooks", Price = 4.5, Quantity = 5 }
						}
				};

				await harness.InputQueueSendEndpoint.Send(msg);

				(await harness.Consumed.Any<InvoiceSubmitted>()).Should().BeTrue();
				(await consumerHarness.Consumed.Any<InvoiceSubmitted>()).Should().BeTrue();
			}
			finally
			{
				await harness.Stop();
			}
		}
	}
}
