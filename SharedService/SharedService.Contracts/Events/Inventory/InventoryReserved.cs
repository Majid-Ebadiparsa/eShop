using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedService.Contracts.Events.Inventory
{
	public record InventoryReserved(Guid OrderId, decimal TotalAmount, string Currency);
}
