namespace DeliveryService.Domain.AggregatesModel
{
	public sealed class Address
	{
		public string Street { get; }
		public string City { get; }
		public string Zip { get; }
		public string Country { get; }

		public Address(string street, string city, string zip, string country)
		{
			if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Street required");

			Street = street; City = city; Zip = zip; Country = country;
		}
	}
}
