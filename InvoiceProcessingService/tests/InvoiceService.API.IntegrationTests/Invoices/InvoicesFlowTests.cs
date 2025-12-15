using FluentAssertions;
using InvoiceService.API.IntegrationTests.Helpers;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.API.IntegrationTests.Invoices
{
	public class InvoicesFlowTests : IClassFixture<TestWebAppFactory>
	{
		private readonly TestWebAppFactory _factory;
		public InvoicesFlowTests(TestWebAppFactory factory) => _factory = factory;

		[Fact]
		public async Task Post_Should_Return_401_When_Missing_Bearer()
		{
			using var client = _factory.CreateClient();
			var invoice = new
			{
				description = "Office supplies",
				dueDate = DateTime.UtcNow.AddDays(7),
				supplier = "ACME GmbH",
				lines = new[] { new { description = "Paper A4", price = 5.99, quantity = 10 } }
			};

			var resp = await client.PostAsJsonAsync("/api/v1/invoices", invoice);
			resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
		}

		[Fact]
		public async Task Post_Should_Persist_And_Publish_Event()
		{
			using var client = _factory.CreateClient();
			var (token, _) = await client.GetDemoTokenAsync();
			client.UseBearer(token);

			var invoice = new
			{
				description = "Laptop stand",
				dueDate = DateTime.UtcNow.AddDays(3),
				supplier = "ACME GmbH",
				lines = new[]
					{
								new { description = "Stand", price = 29.90, quantity = 1 },
								new { description = "Shipping", price = 4.99, quantity = 1 }
						}
			};

			using var scope = _factory.Services.CreateScope();
			var harness = scope.ServiceProvider.GetRequiredService<ITestHarness>();
			await harness.Start();

			try
			{
				var resp = await client.PostAsJsonAsync("/api/v1/invoices", invoice);
				resp.EnsureSuccessStatusCode();

				// Verify a domain event was published
				var published = await harness.Published.Any<Shared.Contracts.Events.InvoiceSubmitted>();
				published.Should().BeTrue();
			}
			finally
			{
				await harness.Stop();
			}
		}

		[Fact]
		public async Task Post_Should_Validate_And_Return_400_For_Bad_Request()
		{
			using var client = _factory.CreateClient();
			var (token, _) = await client.GetDemoTokenAsync();
			client.UseBearer(token);

			// Invalid: empty supplier and no lines
			var bad = new
			{
				description = "",
				dueDate = DateTime.UtcNow.AddDays(-1),
				supplier = "",
				lines = Array.Empty<object>()
			};

			var resp = await client.PostAsJsonAsync("/api/v1/invoices", bad);
			resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
			var body = await resp.Content.ReadAsStringAsync();
			body.Should().ContainAny("Description", "Supplier", "DueDate", "Lines");
		}
	}
}
