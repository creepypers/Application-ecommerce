using Microsoft.AspNetCore.Mvc;
using TPECOM.Models;
using TPECOM.Data;


namespace TPECOM.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index(string sort)
        {
            var products = _context.Products.ToList();
            
            // Trier les produits en fonction du paramètre sort
            products = sort switch
            {
                "newest" => products.OrderByDescending(p => p.CreatedAt).ToList(),
                "price-low" => products.OrderBy(p => p.Price).ToList(),
                "price-high" => products.OrderByDescending(p => p.Price).ToList(),
                "popular" => products.OrderBy(p => Guid.NewGuid()).ToList(), // Simule un tri par popularité aléatoire
                _ => products.OrderBy(p => p.Name).ToList() // Par défaut, trier par nom
            };
            
            // Récupérer les catégories pour l'affichage
            ViewBag.Categories = GetCategories();
            
            return View(products);
        }
        
        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }
            
            return View(product);
        }
        
        public IActionResult Search(string q)
        {
            var products = _context.Products
                .Where(p => p.Name.Contains(q) || p.Description.Contains(q))
                .ToList();
            
            ViewBag.SearchQuery = q;
            
            return View(products);
        }
        
        // Pour la compatibilité avec le code existant, cette méthode retourne tous les produits
        public List<Product> GetSampleProducts()
        {
            return _context.Products.ToList();
        }
        
        // Pour la compatibilité avec le code existant, cette méthode retourne un produit par ID
        public Product GetProductById(int id)
        {
            return _context.Products.FirstOrDefault(p => p.Id == id);
        }
        
        public IActionResult Category(string id)
        {
            // Vérifier si la catégorie existe
            var categoryExists = _context.Products.Any(p => p.Category == id);
            
            if (!categoryExists)
            {
                return NotFound();
            }
            
            // Récupérer tous les produits de cette catégorie
            var products = _context.Products
                .Where(p => p.Category == id)
                .ToList();
            
            ViewBag.CategoryName = id;
            
            return View(products);
        }
        
        // Ajouter cette méthode pour récupérer les catégories distinctes
        public List<string> GetCategories()
        {
            return _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
    }
} 