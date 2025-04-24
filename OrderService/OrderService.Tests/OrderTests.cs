using OrderService.Domain.AggregatesModel;

namespace OrderService.Tests
{
	public class OrderTests
	{
		[Fact]
		public void AddItem_Should_Add_Product_To_Order()
		{
			var order = new Order(Guid.NewGuid(), new Address("Elm St", "Frankfurt", "12345"));
			order.AddItem(Guid.NewGuid(), 2, 100);

			Assert.Single(order.Items);
			Assert.Equal(200, order.TotalAmount);
		}
	}
}
