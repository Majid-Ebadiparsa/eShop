using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Persistence
{
	public class InboxMessage
	{
		public Guid Id { get; set; }                 // MessageId from Header
		public string Consumer { get; set; } = "";   // Consumer/Endpoint Name
		public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
	}
}
