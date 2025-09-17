using Microsoft.AspNetCore.Mvc;
using TPECOM.Models;
using TPECOM.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TPECOM.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        
        public CartController(AppDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            var cartItems = GetCartItems();
            
            // Calculate totals and set ViewBag properties (even if cart is empty)
            decimal subtotal = cartItems.Sum(i => i.Subtotal);
            decimal shipping = subtotal > 0 ? 10.00m : 0;  // Example shipping calculation
            decimal tax = subtotal * 0.07m;  // Example tax calculation (7%)
            
            // Set these values in ViewBag to prevent null reference exceptions
            ViewBag.Subtotal = subtotal;
            ViewBag.Shipping = shipping;
            ViewBag.Tax = tax;
            ViewBag.Total = subtotal + shipping + tax;
            
            return View(cartItems);
        }
        
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var cartItems = GetCartItems();
            
            // Trouver le produit dans la base de données
            var product = _context.Products.Find(productId);
            
            if (product == null)
            {
                TempData["CartMessage"] = "Produit non trouvé.";
                return RedirectToAction("Index");
            }
            
            // Vérifier si le produit est déjà dans le panier
            var existingItem = cartItems.FirstOrDefault(item => item.ProductId == productId);
            
            if (existingItem != null)
            {
                // Mettre à jour la quantité
                existingItem.Quantity += quantity;
            }
            else
            {
                // Ajouter un nouvel article
                cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity,
                    Variant = product.Category // Utiliser la catégorie comme variante pour l'exemple
                });
            }
            
            // Sauvegarder le panier mis à jour
            SaveCartItems(cartItems);
            
            TempData["CartMessage"] = "Produit ajouté au panier avec succès!";
            
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer); 
            }
            
            return RedirectToAction("Index", "Products"); 
        }
        
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cartItems = GetCartItems();
            
            var item = cartItems.FirstOrDefault(i => i.ProductId == productId);
            
            if (item != null)
            {
                if (quantity <= 0)
                {
                    // Supprimer l'article
                    cartItems.Remove(item);
                }
                else
                {
                    // Mettre à jour la quantité
                    item.Quantity = quantity;
                }
                
                SaveCartItems(cartItems);
            }
            
            TempData["CartMessage"] = "Panier mis à jour avec succès!";
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cartItems = GetCartItems();
            var itemToRemove = cartItems.FirstOrDefault(item => item.ProductId == productId);
            
            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                SaveCartItems(cartItems);
                TempData["CartMessage"] = "Produit retiré du panier.";
            }
            
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public IActionResult Clear()
        {
            // Supprimer toutes les entrées du panier dans la session
            for (int i = 0; i < 100; i++) // Limite arbitraire pour éviter une boucle infinie
            {
                if (HttpContext.Session.GetString($"Cart_Item_{i}_ProductId") == null)
                    break;
                    
                HttpContext.Session.Remove($"Cart_Item_{i}_ProductId");
                HttpContext.Session.Remove($"Cart_Item_{i}_Name");
                HttpContext.Session.Remove($"Cart_Item_{i}_ImageUrl");
                HttpContext.Session.Remove($"Cart_Item_{i}_Price");
                HttpContext.Session.Remove($"Cart_Item_{i}_Quantity");
                HttpContext.Session.Remove($"Cart_Item_{i}_Variant");
            }
            
            // Réinitialiser le compteur d'articles
            HttpContext.Session.SetInt32("Cart_Count", 0);
            
            TempData["CartMessage"] = "Votre panier a été vidé.";
            return RedirectToAction("Index");
        }
        
        public List<CartItem> GetCartItems()
        {
            var cartItems = new List<CartItem>();
            
            for (int i = 0; i < 100; i++)
            {
                var productId = HttpContext.Session.GetInt32($"Cart_Item_{i}_ProductId");
                if (productId == null) break;
                
                cartItems.Add(new CartItem
                {
                    ProductId = productId.Value,
                    Name = HttpContext.Session.GetString($"Cart_Item_{i}_Name"),
                    ImageUrl = HttpContext.Session.GetString($"Cart_Item_{i}_ImageUrl"),
                    Price = decimal.Parse(HttpContext.Session.GetString($"Cart_Item_{i}_Price") ?? "0"),
                    Quantity = HttpContext.Session.GetInt32($"Cart_Item_{i}_Quantity") ?? 1,
                    Variant = HttpContext.Session.GetString($"Cart_Item_{i}_Variant")
                });
            }
            
            return cartItems;
        }
        
        private void SaveCartItems(List<CartItem> cartItems)
        {
            // Effacer tous les éléments précédents
            for (int i = 0; i < 100; i++)
            {
                HttpContext.Session.Remove($"Cart_Item_{i}_ProductId");
                HttpContext.Session.Remove($"Cart_Item_{i}_Name");
                HttpContext.Session.Remove($"Cart_Item_{i}_ImageUrl");
                HttpContext.Session.Remove($"Cart_Item_{i}_Price");
                HttpContext.Session.Remove($"Cart_Item_{i}_Quantity");
                HttpContext.Session.Remove($"Cart_Item_{i}_Variant");
            }
            
            // Enregistrer les nouveaux éléments
            for (int i = 0; i < cartItems.Count; i++)
            {
                var item = cartItems[i];
                HttpContext.Session.SetInt32($"Cart_Item_{i}_ProductId", item.ProductId);
                HttpContext.Session.SetString($"Cart_Item_{i}_Name", item.Name);
                HttpContext.Session.SetString($"Cart_Item_{i}_ImageUrl", item.ImageUrl);
                HttpContext.Session.SetString($"Cart_Item_{i}_Price", item.Price.ToString());
                HttpContext.Session.SetInt32($"Cart_Item_{i}_Quantity", item.Quantity);
                HttpContext.Session.SetString($"Cart_Item_{i}_Variant", item.Variant ?? "");
            }
            
            // Mettre à jour le nombre d'articles dans le panier
            HttpContext.Session.SetInt32("Cart_Count", cartItems.Count);
        }
    }
} 