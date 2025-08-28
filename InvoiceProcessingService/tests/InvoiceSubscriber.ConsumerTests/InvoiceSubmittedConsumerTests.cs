using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts.Events;

namespace InvoiceSubscriber.ConsumerTests
{
	public class InvoiceSubmittedConsumerTests
	{
		[Fact]
		public async Task Consumer_Should_Consume_InvoiceSubmitted()
		{
			var services = new ServiceCollection();
			services.AddMassTransitTestHarness(x =>
			{
				x.AddConsumer<InvoiceSubmittedConsumer>();
			});

			await using var provider = services.BuildServiceProvider(true);
			var harness = provider.GetRequiredService<ITestHarness>();
			await harness.Start();

			var bus = provider.GetRequiredService<IBus>();
			await bus.Publish(new InvoiceSubmitted(
					Guid.NewGuid(), "desc", DateTime.UtcNow.AddDays(1), "ACME",
					new[] { new InvoiceLineItem("A", 1.2, 3) }));

			(await harness.Consumed.Any<InvoiceSubmitted>()).Should().BeTrue();

			var consumerHarness = provider.GetRequiredService<IConsumerTestHarness<InvoiceSubmittedConsumer>>();
			(await consumerHarness.Consumed.Any<InvoiceSubmitted>()).Should().BeTrue();
		}
	}
}
