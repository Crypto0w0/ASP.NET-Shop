using ASP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ASP.Controllers
{
    public class ProductController : Controller
    {
        private const string SessionKey = "LastProduct";
        private readonly AppDbContext _context;
        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Group()
        {
            var products = _context.Products.ToList();

            var recommended = _context.Products
                                      .OrderBy(p => Guid.NewGuid())
                                      .Take(4)
                                      .ToList();

            ViewBag.RecommendedProducts = recommended;

            return View(products);
        }


        public IActionResult Group(int? maxPrice = null)
        {
            var products = _context.Products.OrderBy(p => p.Price).ToList();

            var priceLimits = products.Select(p => p.Price).Distinct().OrderBy(p => p).ToList();
            ViewBag.PriceLimits = priceLimits;

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();
                ViewBag.SelectedPrice = maxPrice.Value;
            }

            return View(products);
        }


        [HttpGet]
        public IActionResult Add()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
                return Forbid();
            var sessionData = HttpContext.Session.GetString(SessionKey);
            Product? lastProduct = null;

            if (!string.IsNullOrEmpty(sessionData))
            {
                try
                {
                    lastProduct = JsonSerializer.Deserialize<Product>(sessionData);
                }
                catch
                {
                    HttpContext.Session.Remove(SessionKey);
                }
            }


            ViewBag.LastProduct = lastProduct;
            return View();
        }

        [HttpPost]
        public IActionResult Add(Product product, IFormFile? imageFile)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
                return Forbid();
            if (!ModelState.IsValid)
            {
                var data = HttpContext.Session.GetString(SessionKey);
                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        ViewBag.LastProduct = JsonSerializer.Deserialize<Product>(data);
                    }
                    catch
                    {
                        HttpContext.Session.Remove(SessionKey);
                    }
                }
                return View(product);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadDir = Path.Combine("wwwroot", "images");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                product.ImagePath = "/images/" + uniqueFileName;
            }

            var serialized = JsonSerializer.Serialize(product);
            HttpContext.Session.SetString(SessionKey, serialized);

            return RedirectToAction("Add");
        }
        public IActionResult Recommended()
        {
            var recommended = _context.Products
                                      .OrderBy(p => Guid.NewGuid()) // випадкові товари
                                      .Take(4)
                                      .ToList();

            return View(recommended);
        }
    }
}
