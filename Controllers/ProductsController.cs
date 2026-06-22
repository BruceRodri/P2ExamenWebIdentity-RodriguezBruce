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

namespace NorthwindApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly SakilaContext _context;

        public ProductsController(SakilaContext context)
        {
            _context = context;
        }

        // GET: Products
        /*
        10 PRODUCTOS MAS CAROS
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.UnitPrice)
            .Take(10)
            .ToListAsync();
            return View(productos);
        }
        Mostrar 10 productos junto con el nombre de su categoría usando Include
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Products
            .Include(p => p.Category)
            .Take(10)
            .ToListAsync();
            return View(productos);
        }

        Mostrar 10 productos junto con el nombre de su proveedor usando Include
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Products
            .Include(p => p.Supplier)
            .Take(10)
            .ToListAsync();
            return View(productos);
        }
        
        Mostrar productos pertenecientes a una categoría específica usando Join.
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Products
                .Join(_context.Categories.Where(c => c.CategoryId == 7),
                    p => p.CategoryId,
                    c => c.CategoryId,
                    (p, c) => p)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
            return View(productos);
        }
        */
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Products
            .Include(p => p.Supplier)
            .Take(10)
            .ToListAsync();
            return View(productos);
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
    }
}
