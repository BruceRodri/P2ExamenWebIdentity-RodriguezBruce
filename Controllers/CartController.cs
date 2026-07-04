using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApp.Data;
using NorthwindApp.Models;
using NorthwindApp.Models.ViewModels;
using NorthwindApp.Services;
using System.Linq;
using System.Threading.Tasks;

namespace NorthwindApp.Controllers
{
    [Authorize] // Solo usuarios autenticados pueden usar el carrito
    public class CartController : Controller
    {
        private readonly SakilaContext _context;
        private readonly CartService _cartService;

        public CartController(SakilaContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Cart
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, short quantity)
        {
            // =============================================
            // VALIDACIÓN 1: Cantidad vacía o inválida
            // =============================================
            if (quantity <= 0)
            {
                TempData["Error"] = "❌ Debes ingresar una cantidad válida mayor a cero.";
                return RedirectToAction("Available", "Products");
            }

            // =============================================
            // VALIDACIÓN 2: Producto inexistente
            // =============================================
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                TempData["Error"] = "❌ El producto no existe.";
                return RedirectToAction("Available", "Products");
            }

            // =============================================
            // VALIDACIÓN 5: Producto descontinuado
            // =============================================
            if (product.Discontinued == 1)
            {
                TempData["Error"] = $"❌ '{product.ProductName}' está discontinuado y no puede ser comprado.";
                return RedirectToAction("Available", "Products");
            }

            // =============================================
            // VALIDACIÓN 6: Producto sin existencias
            // =============================================
            var currentStock = product.UnitsInStock ?? 0;
            if (currentStock == 0)
            {
                TempData["Error"] = $"❌ '{product.ProductName}' no tiene stock disponible.";
                return RedirectToAction("Available", "Products");
            }

            // =============================================
            // VALIDACIÓN 7: Cantidad superior al stock
            // =============================================
            if (currentStock < quantity)
            {
                TempData["Error"] = $"❌ Stock insuficiente para '{product.ProductName}'. " +
                                    $"Disponible: {currentStock} unidades.";
                return RedirectToAction("Available", "Products");
            }

            // =============================================
            // VALIDACIÓN 8: Cantidad decimal (no aplica en servidor)
            // =============================================
            // Nota: La cantidad es short, por lo que es entera por defecto

            // =============================================
            // AGREGAR AL CARRITO
            // =============================================
            var cartItem = new CartItemViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                UnitPrice = (decimal)(product.UnitPrice ?? 0),
                Quantity = quantity,
                MaxStock = currentStock,
                Discontinued = product.Discontinued == 1,
                Discount = 0
            };

            _cartService.AddItem(cartItem);

            TempData["Success"] = $"✅ Se agregaron {quantity} unidad(es) de '{product.ProductName}' al carrito.";
            return RedirectToAction("Available", "Products");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(short productId, short quantity)
        {
            // Validación 1: Cantidad no puede ser negativa
            if (quantity < 0)
            {
                return Json(new { success = false, message = "La cantidad no puede ser negativa." });
            }

            // Si cantidad es 0, eliminar
            if (quantity == 0)
            {
                _cartService.RemoveItem(productId);
                return Json(new { success = true, message = "Producto eliminado del carrito." });
            }

            // Validación 2: Verificar stock en la base de datos (concurrencia)
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Producto no encontrado." });
            }

            var currentStock = product.UnitsInStock ?? 0;
            if (currentStock < quantity)
            {
                return Json(new
                {
                    success = false,
                    message = $"Stock insuficiente. Disponible: {currentStock} unidades.",
                    maxStock = currentStock
                });
            }

            // Actualizar cantidad
            _cartService.UpdateQuantity(productId, quantity);

            // Obtener carrito actualizado para recalcular totales
            var cart = _cartService.GetCart();

            return Json(new
            {
                success = true,
                subtotal = cart.Items.FirstOrDefault(i => i.ProductId == productId)?.Subtotal ?? 0,
                total = cart.Total,
                totalItems = cart.TotalItems
            });
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(short productId)
        {
            _cartService.RemoveItem(productId);

            var cart = _cartService.GetCart();
            return Json(new
            {
                success = true,
                total = cart.Total,
                totalItems = cart.TotalItems,
                hasItems = cart.HasItems
            });
        }

        // POST: Cart/ClearCart
        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return Json(new { success = true });
        }

        // GET: Cart/Count
        public IActionResult GetCartCount()
        {
            var cart = _cartService.GetCart();
            return Json(new { count = cart.TotalItems });
        }
    }
}