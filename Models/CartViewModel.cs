using System.Collections.Generic;
using System.Linq;

namespace NorthwindApp.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        public decimal Total => Items.Sum(i => i.Subtotal);

        public int TotalItems => Items.Sum(i => i.Quantity);

        public bool HasItems => Items.Any();

        public decimal Subtotal => Total; // Alias para claridad
    }
}