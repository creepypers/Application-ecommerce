using Microsoft.AspNetCore.Mvc;
using TPECOM.Models;
using TPECOM.Data;
using TPECOM.Services;
using Microsoft.EntityFrameworkCore;



namespace TPECOM.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly StripeService _stripeService;
        private readonly IConfiguration _configuration;
        
        public OrderController(AppDbContext context, StripeService stripeService, IConfiguration configuration)
        {
            _context = context;
            _stripeService = stripeService;
            _configuration = configuration;
        }
        
        private User GetUserById(int id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id);
        }
        
        private void ClearCart()
        {
            for (int i = 0; i < 100; i++)
            {
                HttpContext.Session.Remove($"Cart_Item_{i}_ProductId");
                HttpContext.Session.Remove($"Cart_Item_{i}_Name");
                HttpContext.Session.Remove($"Cart_Item_{i}_ImageUrl");
                HttpContext.Session.Remove($"Cart_Item_{i}_Price");
                HttpContext.Session.Remove($"Cart_Item_{i}_Quantity");
                HttpContext.Session.Remove($"Cart_Item_{i}_Variant");
            }
            
            HttpContext.Session.SetInt32("Cart_Count", 0);
        }
        
        public async Task<IActionResult> Checkout()
        {
            // Vérifier si l'utilisateur est connecté
            var userId = HttpContext.Session.GetInt32("User_Id");
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer les articles du panier directement
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }
            
            // Calculer les totaux
            decimal subtotal = cartItems.Sum(item => item.Subtotal);
            decimal shipping = subtotal > 0 ? 10.00m : 0;
            decimal tax = subtotal * 0.08m; // 8% de taxe
            decimal total = subtotal + shipping + tax;
            
            // Créer un PaymentIntent avec Stripe
            var paymentIntent = await _stripeService.CreatePaymentIntentAsync(total);
            
            // Créer le modèle de vue
            var viewModel = new Checkout
            {
                CartItems = cartItems,
                Subtotal = subtotal,
                Shipping = shipping,
                Tax = tax,
                Total = total,
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                PublicKey = _configuration["Stripe:PublicKey"]
            };
            
            return View(viewModel);
        }
        
        // Méthode pour récupérer les articles du panier
        private List<CartItem> GetCartItems()
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
        
        private OrderItem GetOrderItem(int orderId, int itemId)
        {
            return _context.OrderItems
                .FirstOrDefault(i => i.OrderId == orderId && i.Id == itemId);
        }
        
        // Méthode pour permettre aux autres contrôleurs d'accéder aux commandes
        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Items)
                .ToList();
        }
        
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string shippingAddress, string billingAddress, string paymentIntentId)
        {
            var userId = HttpContext.Session.GetInt32("User_Id");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Vérifier que le PaymentIntent existe et est valide
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                TempData["Error"] = "Une erreur est survenue lors du traitement du paiement.";
                return RedirectToAction("Checkout");
            }
            
            var paymentIntent = await _stripeService.GetPaymentIntentAsync(paymentIntentId);
            if (paymentIntent.Status != "succeeded")
            {
                TempData["Error"] = "Le paiement n'a pas été complété avec succès.";
                return RedirectToAction("Checkout");
            }
            
            // Récupérer les articles du panier
            var cartItems = GetCartItems();
            if (cartItems.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }
            
            // Calculer les totaux
            decimal subtotal = cartItems.Sum(item => item.Subtotal);
            decimal shipping = subtotal > 0 ? 10.00m : 0;
            decimal tax = subtotal * 0.08m;
            decimal total = subtotal + shipping + tax;
            
            // Créer la commande
            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Processing,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress ?? shippingAddress,
                ShippingCost = shipping,
                Tax = tax,
                PaymentIntentId = paymentIntentId,
                Items = cartItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.Name,
                    ProductImageUrl = item.ImageUrl,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Variant = item.Variant
                }).ToList()
            };
            
            // Ajouter la commande à la base de données
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            // Créer la facture
            var invoice = new Models.Invoice
            {
                OrderId = order.Id,
                InvoiceNumber = Models.Invoice.GenerateInvoiceNumber(),
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Status = InvoiceStatus.Paid,
                Order = order
            };
            
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            
            // Vider le panier
            ClearCart();
            
            return RedirectToAction("Confirmation", new { orderId = order.Id });
        }
        
        public IActionResult Confirmation(int orderId)
        {
            // Vérifier si l'utilisateur est connecté
            var userId = HttpContext.Session.GetInt32("User_Id");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Utiliser la méthode existante pour récupérer la commande avec sa facture
            var order = GetOrderById(orderId);
            
            if (order == null || order.UserId != userId)
            {
                return RedirectToAction("History");
            }
            
            return View(order);
        }
        
        public IActionResult History()
        {
            // Vérifier si l'utilisateur est connecté
            var userId = HttpContext.Session.GetInt32("User_Id");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer les commandes de l'utilisateur, inclure les factures
            var orders = _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Invoice)  // S'assurer que les factures sont incluses
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            
            return View(orders);
        }
        
        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("User_Id");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Récupérer la commande
            var order = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);
                
            if (order == null)
            {
                return RedirectToAction("History");
            }
            
            return View(order);
        }
        
        public IActionResult ViewInvoice(int id)
        {
            // Vérifier si l'utilisateur est connecté
            var userId = HttpContext.Session.GetInt32("User_Id");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer la facture avec sa commande et les articles
            var invoice = _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                .FirstOrDefault(i => i.Id == id && i.Order.UserId == userId);
                
            if (invoice == null)
            {
                return RedirectToAction("History");
            }
            
            return View("Invoice", invoice);
        }
        
        public Order GetOrderById(int id)
        {
            return _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Invoice)
                .FirstOrDefault(o => o.Id == id);
        }
        
        private TPECOM.Models.Invoice GetInvoiceForOrder(int orderId)
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.OrderId == orderId);
            
            if (invoice == null)
            {
                var order = GetOrderById(orderId);
                if (order != null)
                {
                    invoice = new Models.Invoice
                    {
                        OrderId = orderId,
                        InvoiceNumber = Models.Invoice.GenerateInvoiceNumber(),
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(30),
                        Status = InvoiceStatus.Paid,
                        Order = order
                    };
                    
                    _context.Invoices.Add(invoice);
                    _context.SaveChanges();
                }
            }
            
            return invoice;
        }
        
        public IActionResult InvoiceDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("User_Id");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var invoice = _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                .FirstOrDefault(i => i.Id == id && i.Order.UserId == userId);
                
            if (invoice == null)
            {
                var userOrders = _context.Orders.Where(o => o.UserId == userId).ToList();
                
                foreach (var order in userOrders)
                {
                    if (_context.Invoices.Any(i => i.OrderId == order.Id))
                    {
                        invoice = GetInvoiceForOrder(order.Id);
                        if (invoice.Id == id) break;
                    }
                }
                
                if (invoice == null)
                {
                    return RedirectToAction("History");
                }
            }
            
            return View(invoice);
        }
        
        public IActionResult Invoice(int id)
        {
            // Vérifier si l'utilisateur est connecté
            var userId = HttpContext.Session.GetInt32("User_Id");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Récupérer la facture avec sa commande et les articles
            var invoice = _context.Invoices
                .Include(i => i.Order)
                .ThenInclude(o => o.Items)
                .FirstOrDefault(i => i.Id == id && i.Order.UserId == userId);
                
            if (invoice == null)
            {
                var userOrders = _context.Orders.Where(o => o.UserId == userId).ToList();
                
                foreach (var order in userOrders)
                {
                    if (_context.Invoices.Any(i => i.OrderId == order.Id))
                    {
                        invoice = GetInvoiceForOrder(order.Id);
                        if (invoice.Id == id) break;
                    }
                }
                
                if (invoice == null)
                {
                    return RedirectToAction("History");
                }
            }
            
            return View("Invoice", invoice);
        }
        
        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            // Récupérer la commande
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == id);
            
            if (order == null)
            {
                return NotFound();
            }
            
            // Vérifier si la commande peut être annulée
            if (order.Status == OrderStatus.Processing || order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Cancelled;
                _context.SaveChanges();
                
                TempData["OrderMessage"] = "Votre commande a été annulée avec succès.";
            }
            else
            {
                TempData["OrderError"] = "Cette commande ne peut pas être annulée car elle a déjà été expédiée ou livrée.";
            }
            
            return RedirectToAction("Details", new { id = id });
        }
    }
} 