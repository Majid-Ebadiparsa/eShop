﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.API.DTOs;
using PaymentService.Application.Payments.Commands;

namespace PaymentService.API.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class PaymentsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public PaymentsController(IMediator mediator) => _mediator = mediator;

		[HttpPost]
		public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto, CancellationToken ct)
		{
			var id = await _mediator.Send(
					new InitiatePaymentCommand(dto.OrderId, dto.Amount, dto.Currency, dto.MethodType), ct);
			return Accepted(new { PaymentId = id });
		}
	}
}
