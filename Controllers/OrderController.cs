using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApp.Data;
using NorthwindApp.Models;
using NorthwindApp.Models.ViewModels;
using NorthwindApp.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NorthwindApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly SakilaContext _context;
        private readonly CartService _cartService;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(SakilaContext context, CartService cartService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
        }

        // GET: Order/Checkout
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();

            // Validación: carrito vacío
            if (!cart.HasItems)
            {
                TempData["Error"] = "El carrito está vacío. Agrega productos antes de continuar.";
                return RedirectToAction("Available", "Products");
            }

            return View(cart);
        }

        // POST: Order/ConfirmOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder()
        {
            // =============================================
            // VALIDACIÓN: Compra sin productos (carrito vacío)
            // =============================================
            var cart = _cartService.GetCart();

            if (!cart.HasItems)
            {
                TempData["Error"] = "❌ No hay productos en el carrito.";
                return RedirectToAction("Available", "Products");
            }

            // =============================================
            // VALIDACIÓN: Acceso sin autenticación
            // =============================================
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // =============================================
            // INICIO DE LA TRANSACCIÓN
            // =============================================
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Generar el siguiente OrderId
                var maxOrderId = await _context.Orders.MaxAsync(o => (short?)o.OrderId) ?? 0;
                var nextOrderId = (short)(maxOrderId + 1);

                // Buscar un cliente existente (si "ALFKI" no existe, usa el primero disponible)
                var customer = await _context.Customers.FindAsync("ALFKI")
                              ?? await _context.Customers.FirstOrDefaultAsync();

                // Crear la orden
                var order = new Order
                {
                    OrderId = nextOrderId,
                    CustomerId = customer?.CustomerId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    RequiredDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                    ShippedDate = null,
                    Freight = 0,
                    ShipName = user.UserName ?? "Cliente",
                    ShipAddress = "Sin dirección",
                    ShipCity = "Sin ciudad",
                    ShipCountry = "Sin país"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal orderTotal = 0;

                foreach (var item in cart.Items)
                {
                    // =============================================
                    // VALIDACIÓN: Verificar stock nuevamente (cambio de stock antes de confirmar)
                    // =============================================
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                    if (product == null)
                    {
                        throw new Exception($"❌ Producto con ID {item.ProductId} no encontrado.");
                    }

                    if (product.Discontinued == 1)
                    {
                        throw new Exception($"❌ El producto '{product.ProductName}' está discontinuado.");
                    }

                    var currentStock = product.UnitsInStock ?? 0;
                    if (currentStock < item.Quantity)
                    {
                        throw new Exception($"❌ Stock insuficiente para '{product.ProductName}'. " +
                            $"Disponible: {currentStock}, Solicitado: {item.Quantity}");
                    }

                    // Actualizar stock
                    product.UnitsInStock = (short?)(currentStock - item.Quantity);

                    // Crear detalle
                    float unitPrice = product.UnitPrice ?? 0;
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = (short)item.ProductId,
                        UnitPrice = unitPrice,
                        Quantity = item.Quantity,
                        Discount = 0f
                    };

                    _context.OrderDetails.Add(orderDetail);
                    orderTotal += (decimal)unitPrice * item.Quantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // =============================================
                // ELIMINACIÓN DE TODOS LOS PRODUCTOS (carrito vacío)
                // =============================================
                _cartService.ClearCart();

                TempData["OrderId"] = (int)order.OrderId;
                TempData["Success"] = $"✅ ¡Orden #{order.OrderId} completada exitosamente! " +
                                      $"Total: {orderTotal.ToString("C")}";

                return RedirectToAction("OrderSummary", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                // =============================================
                // ERROR DURANTE EL REGISTRO (Rollback)
                // =============================================
                await transaction.RollbackAsync();

                var logger = HttpContext.RequestServices.GetService<ILogger<OrderController>>();
                logger?.LogError(ex, "Error al procesar la orden");

                TempData["Error"] = "❌ Ocurrió un error al procesar su compra. " +
                                    "Por favor, intente nuevamente.";

                return RedirectToAction("Checkout");
            }
        }

        // GET: Order/OrderSummary/5
        public async Task<IActionResult> OrderSummary(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");

            return View(order);
        }

        // GET: Order/AllOrders (Solo Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            var isAdmin = User.IsInRole("Admin");

            return View(order);
        }
    }
}