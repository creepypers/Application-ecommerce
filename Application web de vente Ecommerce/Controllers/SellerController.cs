using Microsoft.AspNetCore.Mvc;
using TPECOM.Models;
using TPECOM.Data;
using Microsoft.EntityFrameworkCore;

namespace TPECOM.Controllers
{
    public class SellerController : Controller
    {
        private readonly AppDbContext _context;
        
        public SellerController(AppDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Dashboard()
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer les produits du vendeur
            var products = _context.Products.Where(p => p.SellerId == userId).ToList();
            
            return View(products);
        }
        
        public IActionResult AddProduct()
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur")
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer les catégories pour l'affichage
            ViewBag.Categories = _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            
            return View(new Product());
        }
        
        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            if (!ModelState.IsValid)
            {
                return View(product);
            }
            
            // Définir le SellerId et la date de création
            product.SellerId = userId.Value;
            product.CreatedAt = DateTime.Now;
            
            // Ajouter le produit à la base de données
            _context.Products.Add(product);
            _context.SaveChanges();
            
            // Rediriger vers le tableau de bord
            TempData["SuccessMessage"] = "Produit ajouté avec succès !";
            return RedirectToAction("Dashboard");
        }
        
        public IActionResult EditProduct(int id)
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer le produit
            var product = _context.Products.FirstOrDefault(p => p.Id == id && p.SellerId == userId);
            
            if (product == null)
            {
                return NotFound();
            }
            
            // Récupérer les catégories pour l'affichage
            ViewBag.Categories = _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            
            return View(product);
        }
        
        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // En cas d'erreur, il faut également charger les catégories
                ViewBag.Categories = _context.Products
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
                
                return View(product);
            }
            
            // Vérifier que le produit appartient au vendeur
            var existingProduct = _context.Products.FirstOrDefault(p => p.Id == product.Id && p.SellerId == userId);
            
            if (existingProduct == null)
            {
                return NotFound();
            }
            
            // Mettre à jour le produit
            existingProduct.Name = product.Name;
            existingProduct.ShortDescription = product.ShortDescription;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.ImageUrl = product.ImageUrl;
            
            _context.Products.Update(existingProduct);
            _context.SaveChanges();
            
            // Rediriger vers le tableau de bord
            TempData["SuccessMessage"] = "Produit mis à jour avec succès !";
            return RedirectToAction("Dashboard");
        }
        
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Vérifier que le produit appartient au vendeur
            var product = _context.Products.FirstOrDefault(p => p.Id == id && p.SellerId == userId);
            
            if (product == null)
            {
                return NotFound();
            }
            
            // Supprimer le produit
            _context.Products.Remove(product);
            _context.SaveChanges();
            
            // Rediriger vers le tableau de bord
            TempData["SuccessMessage"] = "Produit supprimé avec succès !";
            return RedirectToAction("Dashboard");
        }
        
        public IActionResult Orders()
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer les commandes contenant des produits du vendeur
            var sellerOrders = _context.Orders
                .Include(o => o.Items)
                .Where(o => o.Items.Any(item => item.SellerId == userId))
                .ToList();
            
            return View(sellerOrders);
        }
        
        public IActionResult OrderDetails(int id)
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer la commande
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Vérifier que le vendeur a des produits dans cette commande
            var sellerItems = order.Items.Where(item => item.SellerId == userId).ToList();
            if (sellerItems.Count == 0)
            {
                return RedirectToAction("Orders");
            }
            
            // Créer un modèle pour la vue avec seulement les articles du vendeur
            var sellerOrder = new Order
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                ShippingCost = order.ShippingCost,
                Tax = order.Tax,
                Items = sellerItems
            };
            
            return View(sellerOrder);
        }
        
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, int itemId, OrderStatus status)
        {
            // Vérifier si l'utilisateur est connecté et est un vendeur
            var userType = HttpContext.Session.GetString("User_Type");
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (string.IsNullOrEmpty(userType) || userType != "Vendeur" || userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer l'article de commande
            var orderItem = _context.OrderItems
                .FirstOrDefault(i => i.Id == itemId && i.OrderId == orderId && i.SellerId == userId);
            
            if (orderItem == null)
            {
                return NotFound();
            }
            
            // Mettre à jour le statut de l'article
            orderItem.Status = status;
            _context.OrderItems.Update(orderItem);
            _context.SaveChanges();
            
            // Rediriger vers les détails de la commande
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
    }
} 