using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregatesModel
{
	public record Address(string Street, string City, string PostalCode) : IValueObject;
}
