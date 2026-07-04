namespace NorthwindApp.Models.ViewModels
{
    public class MostPurchasedViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Category? Category { get; set; }
        public Supplier? Supplier { get; set; }
        public decimal UnitPrice { get; set; }
        public short UnitsInStock { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
