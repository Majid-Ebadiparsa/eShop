using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public abstract class BaseApiController : ControllerBase
	{
		private IMediator _mediator;
		protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>();
	}
}
