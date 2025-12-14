using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Abstractions
{
	public interface IIntegrationEventPublisher
	{
		Task PublishAsync<T>(T @event, CancellationToken ct);
	}
}
