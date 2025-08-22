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
			if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City required");
			if (string.IsNullOrWhiteSpace(zip)) throw new ArgumentException("Zip required");
			if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country required");


			Street = street; City = city; Zip = zip; Country = country;
		}
	}
}
