using InvoiceService.API.DTOs;
using InvoiceService.Application.Invoices.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceService.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	[ApiVersion("1.0")]
	public class InvoicesController : ControllerBase
	{
		private readonly ISender _sender;


		public InvoicesController(ISender sender) => _sender = sender;


		[HttpPost]
		public async Task<IActionResult> Submit([FromBody] InvoiceDto dto, CancellationToken ct)
		{
			var cmd = new SubmitInvoiceCommand(
			dto.Description,
			dto.DueDate,
			dto.Supplier,
			dto.Lines.Select(l => new SubmitInvoiceLine(l.Description, l.Price, l.Quantity)).ToList());


			var id = await _sender.Send(cmd, ct);
			return CreatedAtAction(nameof(GetById), new { id }, new { id });
		}


		// Simple for Demo – for testing retrieval
		[HttpGet("{id:guid}")]
		public IActionResult GetById(Guid id) => Ok(new { id });
	}
}
