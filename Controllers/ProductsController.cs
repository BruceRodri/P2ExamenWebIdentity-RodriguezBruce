using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NorthwindApp.Data;
using NorthwindApp.Models;
using NorthwindApp.Models.ViewModels;

namespace NorthwindApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly SakilaContext _context;

        public ProductsController(SakilaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Products
            .Include(p => p.Supplier)
            .Take(10)
            .ToListAsync();
            return View(productos);
        }

        //AVAILABLE PRODUCTS
        // GET: Products/Available
        [AllowAnonymous] // Cualquier usuario puede ver los productos
        public async Task<IActionResult> Available(string searchString)
        {
            // Consulta LINQ: Productos disponibles para compra
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.UnitsInStock.HasValue &&
                            p.UnitsInStock > 0 &&
                            p.Discontinued == 0)
                .AsQueryable();

            // Búsqueda por nombre (opcional)
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(searchString.ToLower()));
            }

            // Ordenar alfabéticamente
            products = products.OrderBy(p => p.ProductName);

            return View(await products.ToListAsync());
        }
        // GET: Products/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,SupplierId,CategoryId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", product.SupplierId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", product.SupplierId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("ProductId,ProductName,SupplierId,CategoryId,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierId", product.SupplierId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(short id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        // GET: Products/ManageStock/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageStock(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/IncreaseStock
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseStock(short productId, short quantity)
        {
            // Validación: cantidad debe ser positiva
            if (quantity <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction("ManageStock", new { id = productId });
            }

            // Buscar el producto
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Admin", "Products");
            }

            // Incrementar stock (manejar posible nulo)
            product.UnitsInStock = (short?)((product.UnitsInStock ?? 0) + quantity);

            // Guardar cambios
            await _context.SaveChangesAsync();

            // Mensaje de éxito
            TempData["Success"] = $"✅ Stock de '{product.ProductName}' incrementado en {quantity} unidades. " +
                                  $"Nuevo stock: {product.UnitsInStock} unidades.";

            return RedirectToAction("Admin", "Products");
        }

        // POST: Products/DecreaseStock
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseStock(short productId, short quantity)
        {
            // Validación: cantidad debe ser positiva
            if (quantity <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction("ManageStock", new { id = productId });
            }

            // Buscar el producto
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Admin", "Products");
            }

            // Validación: no se puede reducir más stock del que existe
            var currentStock = product.UnitsInStock ?? 0;
            if (currentStock < quantity)
            {
                TempData["Error"] = $"❌ No se puede reducir. Stock actual: {currentStock} unidades. " +
                                    $"La reducción solicitada ({quantity}) excede el stock disponible.";
                return RedirectToAction("ManageStock", new { id = productId });
            }

            // Reducir stock
            product.UnitsInStock = (short?)(currentStock - quantity);

            // Guardar cambios
            await _context.SaveChangesAsync();

            // Mensaje de éxito
            TempData["Success"] = $"✅ Stock de '{product.ProductName}' reducido en {quantity} unidades. " +
                                  $"Nuevo stock: {product.UnitsInStock} unidades.";

            return RedirectToAction("Admin", "Products");
        }

        // GET: Products/Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin(string searchString)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(searchString.ToLower()));
            }

            // Ordenar por nombre
            products = products.OrderBy(p => p.ProductName);

            return View(await products.ToListAsync());
        }

        // GET: Products/LowStock
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LowStock()
        {
            // LINQ: Productos con stock bajo (1-10 unidades), no discontinuados
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.UnitsInStock.HasValue &&
                            p.UnitsInStock > 0 &&
                            p.UnitsInStock <= 10 &&
                            p.Discontinued == 0)
                .OrderBy(p => p.UnitsInStock)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/OutOfStock
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OutOfStock()
        {
            // LINQ: Productos sin stock (UnitsInStock = 0), no discontinuados
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.UnitsInStock.HasValue &&
                            p.UnitsInStock == 0 &&
                            p.Discontinued == 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Discontinued
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Discontinued()
        {
            // LINQ: Productos discontinuados (Discontinued = 1)
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => p.Discontinued == 1)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            return View(products);
        }

        // GET: Products/MostPurchased
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MostPurchased(int top = 10)
        {
            var mostPurchased = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.UnitPrice * od.Quantity)
                })
                .OrderByDescending(g => g.TotalQuantity)
                .Take(top)
                .Join(_context.Products,
                      g => g.ProductId,
                      p => p.ProductId,
                      (g, p) => new MostPurchasedViewModel
                      {
                          ProductId = p.ProductId,
                          ProductName = p.ProductName,
                          Category = p.Category,
                          Supplier = p.Supplier,
                          UnitPrice = (decimal)(p.UnitPrice ?? 0),
                          UnitsInStock = p.UnitsInStock ?? 0,
                          TotalQuantitySold = g.TotalQuantity,
                          TotalRevenue = (decimal)g.TotalRevenue
                      })
                .ToListAsync();

            return View(mostPurchased);
        }
    }
}
