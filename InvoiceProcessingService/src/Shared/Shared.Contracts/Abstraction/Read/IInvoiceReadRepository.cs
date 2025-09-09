using Shared.Contracts.Invoices.Queries.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Contracts.Abstraction.Read
{
	// TODO: Should be better add this interface in the Application layer of the InvoiceService
	// but for incompatibility .Net versions between projects, I add it here temporarily
	public interface IInvoiceReadRepository
	{
		Task<List<InvoiceDto>> GetAllAsync();
		Task<InvoiceDto> GetByIdAsync(string id);
		Task AddAsync(InvoiceDto invoice);
	}
}
