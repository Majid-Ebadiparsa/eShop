using OrderService.Domain.AggregatesModel;
using OrderService.Domain.SeedWork;

namespace OrderService.Tests
{
	public class OrderTests
	{
		[Fact]
		public void Create_Order_Should_Initialize_With_Pending_Status()
		{
			// Arrange
			var customerId = Guid.NewGuid();
			var address = new Address("Elm St", "Frankfurt", "12345");

			// Act
			var order = new Order(customerId, address);

			// Assert
			Assert.Equal(customerId, order.CustomerId);
			Assert.Equal(OrderStatus.Pending, order.Status);
			Assert.Empty(order.Items);
			Assert.Equal(0, order.TotalAmount);
			Assert.Equal(address, order.ShippingAddress);
		}

		[Fact]
		public void AddItem_Should_Add_Product_To_Order()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));
			var productId = Guid.NewGuid();

			// Act
			order.AddItem(productId, 2, 100);

			// Assert
			Assert.Single(order.Items);
			Assert.Equal(200, order.TotalAmount);
			Assert.Equal(productId, order.Items.First().ProductId);
			Assert.Equal(2, order.Items.First().Quantity);
			Assert.Equal(100, order.Items.First().UnitPrice);
		}

		[Fact]
		public void AddItem_Multiple_Products_Should_Calculate_Total_Correctly()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));

			// Act
			order.AddItem(Guid.NewGuid(), 2, 100); // 200
			order.AddItem(Guid.NewGuid(), 3, 50);  // 150
			order.AddItem(Guid.NewGuid(), 1, 75);  // 75

			// Assert
			Assert.Equal(3, order.Items.Count);
			Assert.Equal(425, order.TotalAmount); // 200 + 150 + 75
		}

		[Fact]
		public void AddItem_With_Zero_Quantity_Should_Throw_Exception()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				order.AddItem(Guid.NewGuid(), 0, 100));
			Assert.Contains("Quantity must be greater than zero", exception.Message);
		}

		[Fact]
		public void AddItem_With_Negative_Quantity_Should_Throw_Exception()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				order.AddItem(Guid.NewGuid(), -1, 100));
			Assert.Contains("Quantity must be greater than zero", exception.Message);
		}

		[Fact]
		public void AddItem_With_Zero_Price_Should_Throw_Exception()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => 
				order.AddItem(Guid.NewGuid(), 1, 0));
			Assert.Contains("Unit price must be greater than zero", exception.Message);
		}

		[Fact]
		public void MarkAsProcessing_Should_Change_Status_From_Pending()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));
			order.AddItem(Guid.NewGuid(), 1, 100);

			// Act
			order.MarkAsProcessing();

			// Assert
			Assert.Equal(OrderStatus.Processing, order.Status);
		}

		[Fact]
		public void MarkAsShipped_Should_Change_Status_From_Processing()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));
			order.AddItem(Guid.NewGuid(), 1, 100);
			order.MarkAsProcessing();

			// Act
			order.MarkAsShipped();

			// Assert
			Assert.Equal(OrderStatus.Shipped, order.Status);
		}

		[Fact]
		public void MarkAsDelivered_Should_Change_Status_From_Shipped()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));
			order.AddItem(Guid.NewGuid(), 1, 100);
			order.MarkAsProcessing();
			order.MarkAsShipped();

			// Act
			order.MarkAsDelivered();

			// Assert
			Assert.Equal(OrderStatus.Delivered, order.Status);
		}

		[Fact]
		public void MarkAsCancelled_Should_Change_Status_To_Cancelled()
		{
			// Arrange
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));
			order.AddItem(Guid.NewGuid(), 1, 100);

			// Act
			order.MarkAsCancelled();

			// Assert
			Assert.Equal(OrderStatus.Cancelled, order.Status);
		}

		[Fact]
		public void Address_Should_Have_Required_Fields()
		{
			// Arrange & Act
			var address = new Address("123 Main St", "New York", "10001");

			// Assert
			Assert.Equal("123 Main St", address.Street);
			Assert.Equal("New York", address.City);
			Assert.Equal("10001", address.ZipCode);
		}

		[Theory]
		[InlineData("", "City", "12345")]
		[InlineData("Street", "", "12345")]
		[InlineData("Street", "City", "")]
		public void Address_With_Empty_Fields_Should_Throw_Exception(string street, string city, string zipCode)
		{
			// Act & Assert
			Assert.Throws<ArgumentException>(() => new Address(street, city, zipCode));
		}

		[Fact]
		public void Order_Should_Track_Creation_Date()
		{
			// Arrange
			var before = DateTime.UtcNow;

			// Act
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));

			// Assert
			var after = DateTime.UtcNow;
			Assert.True(order.CreatedAt >= before && order.CreatedAt <= after);
		}
	}
}
