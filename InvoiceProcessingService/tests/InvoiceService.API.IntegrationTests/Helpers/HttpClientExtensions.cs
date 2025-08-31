using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace InvoiceService.API.IntegrationTests.Helpers
{
	public static class HttpClientExtensions
	{
		public static void UseBearer(this HttpClient client, string token)
				=> client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

		public static async Task<(string accessToken, int expiresIn)> GetDemoTokenAsync(this HttpClient client)
		{
			var resp = await client.PostAsJsonAsync("/api/v1/auth/token", new { username = "demo", password = "Passw0rd!" });
			resp.EnsureSuccessStatusCode();
			var dto = await resp.Content.ReadFromJsonAsync<TokenDto>();
			return (dto!.accessToken, dto.expiresIn);
		}

		public sealed record TokenDto(string accessToken, int expiresIn);
	}
}
