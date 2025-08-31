using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;

namespace InventoryService.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddMediatR(this IServiceCollection services)
		{
			services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
			services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
			return services;
		}
	}
}
