using Swashbuckle.AspNetCore.Filters;

namespace PaymentService.API.DTOs
{
	public class LoginRequestExample : IExamplesProvider<LoginRequest>
	{
		public LoginRequest GetExamples()
		{
			return new LoginRequest("demo", "Passw0rd!");
		}
	}
}
