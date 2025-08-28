using FluentAssertions;
using InvoiceService.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace InvoiceService.API.IntegrationTests
{
	public class InvoiceApiTests : IClassFixture<CustomWebAppFactory>
	{
		private readonly CustomWebAppFactory _factory;
		public InvoiceApiTests(CustomWebAppFactory factory) => _factory = factory;

		private record LoginRequest(string Username, string Password);
		private record TokenResponse(string access_token, DateTime expires);
		private record InvoiceLineDto(string Description, double Price, int Quantity);
		private record InvoiceDto(string Description, DateTime DueDate, string Supplier, List<InvoiceLineDto> Lines);

		[Fact]
		public async Task Auth_Then_Post_Invoice_Should_Return_201_And_Publish()
		{
			var client = _factory.CreateClient();

			// 1) Token
			var auth = await client.PostAsJsonAsync("/api/auth/token", new LoginRequest("demo", "Passw0rd!"));
			auth.IsSuccessStatusCode.Should().BeTrue();
			var token = await auth.Content.ReadFromJsonAsync<TokenResponse>();
			client.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.access_token);

			// 2) Submit invoice
			var dto = new InvoiceDto("Office", DateTime.UtcNow.AddDays(2), "Amazon",
					new() { new("Pens", 1.2, 10) });
			var resp = await client.PostAsJsonAsync("/api/invoices", dto);
			resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

			// 3) Published?
			var ep = (InMemoryEventPublisher)_factory.Services.GetRequiredService<IEventPublisher>();
			ep.Published.Should().HaveCount(1);
			ep.Published[0].Supplier.Should().Be("Amazon");
		}

		[Fact]
		public async Task Post_Invoice_Without_Token_Should_Be_401()
		{
			var client = _factory.CreateClient();
			var dto = new
			{
				description = "x",
				dueDate = DateTime.UtcNow.AddDays(1),
				supplier = "ACME",
				lines = new[] { new { description = "A", price = 1.0, quantity = 1 } }
			};

			var resp = await client.PostAsJsonAsync("/api/invoices", dto);
			resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
		}
	}
}
