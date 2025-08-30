using FluentAssertions;
using InvoiceService.API.IntegrationTests.Helpers;

namespace InvoiceService.API.IntegrationTests.Auth
{
	public class AuthFlowTests : IClassFixture<TestWebAppFactory>
	{
		private readonly TestWebAppFactory _factory;
		public AuthFlowTests(TestWebAppFactory factory) => _factory = factory;

		[Fact]
		public async Task Should_Issue_Jwt_Token_For_Demo_User()
		{
			using var client = _factory.CreateClient();
			var (token, expires) = await client.GetDemoTokenAsync();

			token.Should().NotBeNullOrWhiteSpace();
			expires.Should().BeGreaterThan(0);
		}
	}
}
