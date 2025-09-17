using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TPECOM.Models;
using TPECOM.Data;

namespace TPECOM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Récupérer les catégories distinctes pour l'affichage
            ViewBag.Categories = _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c) 
                .ToList();
            
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public IActionResult Products()
        {
            return RedirectToAction("Index", "Products");
        }
    }
}
