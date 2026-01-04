namespace OrderService.Infrastructure.Projections
{
    public class OrderView
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }
        public List<OrderItemView> Items { get; set; } = new();
        public string Status { get; set; } = "NEW";
    }

    public class OrderItemView
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
