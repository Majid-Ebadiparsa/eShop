using DeliveryService.Domain.AggregatesModel;
using DeliveryService.Domain.SeedWork;

namespace DeliveryService.Tests.Domain
{
	public class ShipmentTests
	{
		[Fact]
		public void Create_Shipment_Should_Initialize_With_Created_Status()
		{
			// Arrange
			var orderId = Guid.NewGuid();
			var address = new Address("123 Main St", "New York", "10001", "USA");
			var items = new List<ShipmentItem>
			{
				new ShipmentItem(Guid.NewGuid(), 2)
			};

			// Act
			var shipment = Shipment.Create(orderId, address, items);

			// Assert
			Assert.Equal(orderId, shipment.OrderId);
			Assert.Equal(ShipmentStatus.Created, shipment.Status);
			Assert.Equal(address, shipment.Address);
			Assert.Single(shipment.Items);
		}

		[Fact]
		public void Create_With_Empty_OrderId_Should_Throw_Exception()
		{
			// Arrange
			var address = new Address("123 Main St", "New York", "10001", "USA");
			var items = new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) };

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				Shipment.Create(Guid.Empty, address, items));
			Assert.Contains("OrderId is required", exception.Message);
		}

		[Fact]
		public void MarkBooked_Should_Update_Status_And_Set_Carrier_Info()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);
			var carrier = "FedEx";
			var trackingNumber = "TRACK123456";

			// Act
			shipment.MarkBooked(carrier, trackingNumber);

			// Assert
			Assert.Equal(ShipmentStatus.LabelBooked, shipment.Status);
			Assert.Equal(carrier, shipment.Carrier);
			Assert.Equal(trackingNumber, shipment.TrackingNumber);
		}

		[Fact]
		public void MarkBooked_From_Non_Created_Status_Should_Throw_Exception()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);
			shipment.MarkBooked("FedEx", "TRACK123");

			// Act & Assert
			var exception = Assert.Throws<InvalidOperationException>(() => 
				shipment.MarkBooked("UPS", "TRACK456"));
			Assert.Contains("Invalid state", exception.Message);
		}

		[Fact]
		public void MarkDispatched_Should_Update_Status()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);
			shipment.MarkBooked("FedEx", "TRACK123");

			// Act
			shipment.MarkDispatched();

			// Assert
			Assert.Equal(ShipmentStatus.Dispatched, shipment.Status);
		}

		[Fact]
		public void MarkDispatched_From_Non_Booked_Status_Should_Throw_Exception()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);

			// Act & Assert
			var exception = Assert.Throws<InvalidOperationException>(() => 
				shipment.MarkDispatched());
			Assert.Contains("Invalid state", exception.Message);
		}

		[Fact]
		public void MarkDelivered_From_Dispatched_Should_Update_Status()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);
			shipment.MarkBooked("FedEx", "TRACK123");
			shipment.MarkDispatched();

			// Act
			shipment.MarkDelivered();

			// Assert
			Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
		}

		[Fact]
		public void MarkDelivered_From_Created_Should_Throw_Exception()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);

			// Act & Assert
			var exception = Assert.Throws<InvalidOperationException>(() => 
				shipment.MarkDelivered());
			Assert.Contains("Invalid state", exception.Message);
		}

		[Fact]
		public void MarkBookingFailed_Should_Set_Status_And_Reason()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);
			var reason = "Invalid address";

			// Act
			shipment.MarkBookingFailed(reason);

			// Assert
			Assert.Equal(ShipmentStatus.BookingFailed, shipment.Status);
			Assert.Equal(reason, shipment.Reason);
		}

		[Fact]
		public void MarkBookingFailed_From_Non_Created_Status_Should_Throw_Exception()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);
			shipment.MarkBooked("FedEx", "TRACK123");

			// Act & Assert
			var exception = Assert.Throws<InvalidOperationException>(() => 
				shipment.MarkBookingFailed("Too late"));
			Assert.Contains("Invalid state", exception.Message);
		}

		[Fact]
		public void Shipment_Should_Have_Multiple_Items()
		{
			// Arrange
			var items = new List<ShipmentItem>
			{
				new ShipmentItem(Guid.NewGuid(), 2),
				new ShipmentItem(Guid.NewGuid(), 3),
				new ShipmentItem(Guid.NewGuid(), 1)
			};

			// Act
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				items
			);

			// Assert
			Assert.Equal(3, shipment.Items.Count);
		}

		[Fact]
		public void Shipment_Should_Track_Creation_Time()
		{
			// Arrange
			var before = DateTime.UtcNow;

			// Act
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);

			// Assert
			var after = DateTime.UtcNow;
			Assert.True(shipment.CreatedAtUtc >= before && shipment.CreatedAtUtc <= after);
		}

		[Fact]
		public void Status_Changes_Should_Update_UpdatedAtUtc()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);
			var initialUpdateTime = shipment.UpdatedAtUtc;
			Thread.Sleep(10); // Ensure time difference

			// Act
			shipment.MarkBooked("FedEx", "TRACK123");

			// Assert
			Assert.True(shipment.UpdatedAtUtc > initialUpdateTime);
		}

		[Theory]
		[InlineData("", "City", "12345", "USA")]
		[InlineData("Street", "", "12345", "USA")]
		[InlineData("Street", "City", "", "USA")]
		[InlineData("Street", "City", "12345", "")]
		public void Address_With_Empty_Fields_Should_Throw_Exception(string street, string city, string zip, string country)
		{
			// Act & Assert
			Assert.Throws<ArgumentException>(() => new Address(street, city, zip, country));
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		[InlineData(-10)]
		public void ShipmentItem_With_Invalid_Quantity_Should_Throw_Exception(int quantity)
		{
			// Act & Assert
			Assert.Throws<ArgumentException>(() => 
				new ShipmentItem(Guid.NewGuid(), quantity));
		}

		[Fact]
		public void Complete_Shipment_Lifecycle_Should_Work()
		{
			// Arrange
			var shipment = Shipment.Create(
				Guid.NewGuid(),
				new Address("123 Main St", "New York", "10001", "USA"),
				new List<ShipmentItem> { new ShipmentItem(Guid.NewGuid(), 1) }
			);

			// Act & Assert - Full lifecycle
			Assert.Equal(ShipmentStatus.Created, shipment.Status);

			shipment.MarkBooked("FedEx", "TRACK123");
			Assert.Equal(ShipmentStatus.LabelBooked, shipment.Status);

			shipment.MarkDispatched();
			Assert.Equal(ShipmentStatus.Dispatched, shipment.Status);

			shipment.MarkDelivered();
			Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
		}
	}
}

