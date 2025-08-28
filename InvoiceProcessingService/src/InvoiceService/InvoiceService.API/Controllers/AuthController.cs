using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvoiceService.API.Controllers
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _cfg;


		public AuthController(IConfiguration cfg) => _cfg = cfg;


		public record LoginRequest(string Username, string Password);


		[HttpPost("token")]
		public IActionResult Token([FromBody] LoginRequest req)
		{
			// Demo only – replace with real user store
			if (req.Username != "demo" || req.Password != "Passw0rd!")
				return Unauthorized();


			var issuer = _cfg["Jwt:Issuer"]!;
			var audience = _cfg["Jwt:Audience"]!;
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:ExpiryMinutes"]!));


			var token = new JwtSecurityToken(
			issuer: issuer,
			audience: audience,
			claims: new[] { new Claim(ClaimTypes.Name, req.Username) },
			expires: expires,
			signingCredentials: creds);


			return Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(token), expires });
		}
	}
}
