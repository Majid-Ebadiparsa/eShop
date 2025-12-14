using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PaymentService.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPaymentApplication(this IServiceCollection services)
		{
			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
			//services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
			return services;
		}
	}
}
