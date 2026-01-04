using InventoryService.Domain.AggregatesModel;

namespace InventoryService.Tests.Domain
{
	public class InventoryItemTests
	{
		[Fact]
		public void Create_InventoryItem_Should_Initialize_With_Correct_Values()
		{
			// Arrange
			var id = Guid.NewGuid();
			var productId = Guid.NewGuid();
			var initialQuantity = 100;

			// Act
			var item = new InventoryItem(id, productId, initialQuantity);

			// Assert
			Assert.Equal(id, item.Id);
			Assert.Equal(productId, item.ProductId);
			Assert.Equal(initialQuantity, item.Quantity);
		}

		[Fact]
		public void Decrease_Should_Reduce_Quantity()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act
			item.Decrease(30);

			// Assert
			Assert.Equal(70, item.Quantity);
		}

		[Fact]
		public void Decrease_Multiple_Times_Should_Accumulate()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act
			item.Decrease(20);
			item.Decrease(15);
			item.Decrease(10);

			// Assert
			Assert.Equal(55, item.Quantity);
		}

		[Fact]
		public void Decrease_With_Zero_Should_Throw_Exception()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => item.Decrease(0));
			Assert.Contains("Quantity must be greater than 0", exception.Message);
		}

		[Fact]
		public void Decrease_With_Negative_Should_Throw_Exception()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => item.Decrease(-10));
			Assert.Contains("Quantity must be greater than 0", exception.Message);
		}

		[Fact]
		public void Decrease_More_Than_Available_Should_Throw_Exception()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 50);

			// Act & Assert
			var exception = Assert.Throws<InvalidOperationException>(() => item.Decrease(100));
			Assert.Contains("Not enough inventory", exception.Message);
		}

		[Fact]
		public void Decrease_To_Zero_Should_Succeed()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 50);

			// Act
			item.Decrease(50);

			// Assert
			Assert.Equal(0, item.Quantity);
		}

		[Fact]
		public void Increase_Should_Add_To_Quantity()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act
			item.Increase(50);

			// Assert
			Assert.Equal(150, item.Quantity);
		}

		[Fact]
		public void Increase_Multiple_Times_Should_Accumulate()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act
			item.Increase(25);
			item.Increase(30);
			item.Increase(45);

			// Assert
			Assert.Equal(200, item.Quantity);
		}

		[Fact]
		public void Increase_With_Zero_Should_Throw_Exception()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => item.Increase(0));
			Assert.Contains("Quantity must be greater than 0", exception.Message);
		}

		[Fact]
		public void Increase_With_Negative_Should_Throw_Exception()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act & Assert
			var exception = Assert.Throws<ArgumentException>(() => item.Increase(-25));
			Assert.Contains("Quantity must be greater than 0", exception.Message);
		}

		[Fact]
		public void Decrease_And_Increase_Should_Work_Together()
		{
			// Arrange
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), 100);

			// Act
			item.Decrease(30); // 70
			item.Increase(20); // 90
			item.Decrease(40); // 50
			item.Increase(50); // 100

			// Assert
			Assert.Equal(100, item.Quantity);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(50)]
		[InlineData(1000)]
		public void Create_With_Various_Initial_Quantities_Should_Succeed(int quantity)
		{
			// Act
			var item = new InventoryItem(Guid.NewGuid(), Guid.NewGuid(), quantity);

			// Assert
			Assert.Equal(quantity, item.Quantity);
		}
	}
}

