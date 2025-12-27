namespace InventoryService.Infrastructure.Projections
{
    public class InventoryView
    {
        public Guid ProductId { get; set; }
        public int Available { get; set; }
        public int Reserved { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
