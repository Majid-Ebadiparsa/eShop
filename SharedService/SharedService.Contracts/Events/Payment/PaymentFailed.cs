using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedService.Contracts.Events.Payment
{
	public record PaymentFailed(Guid OrderId, string Reason);
}
