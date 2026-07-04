using System.ComponentModel.DataAnnotations;

namespace NorthwindApp.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public short Quantity { get; set; }

        public decimal Subtotal => UnitPrice * Quantity;

        public int MaxStock { get; set; }

        public bool Discontinued { get; set; }

        public decimal? Discount { get; set; }
    }
}