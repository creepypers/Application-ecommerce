using TPECOM.Data;
using TPECOM.Models;

namespace TPECOM.Services
{
    public class DbInitializer
    {
        private readonly AppDbContext _context;
        private readonly DummyJsonService _dummyJsonService;

        public DbInitializer(AppDbContext context, DummyJsonService dummyJsonService)
        {
            _context = context;
            _dummyJsonService = dummyJsonService;
        }

        public async Task InitializeAsync()
        {
            // S'assurer que la base de données est créée
            await _context.Database.EnsureCreatedAsync();

            // Vérifier si la base de données est déjà initialisée
            if (_context.Users.Any())
            {
                return; // La base de données est déjà peuplée
            }

            // Récupérer les utilisateurs
            var users = await _dummyJsonService.GetUsersAsync();
            
            foreach (var user in users)
            {
                user.Id = 0; 
            }
            
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Récupérer et sauvegarder les produits
            var products = await _dummyJsonService.GetProductsAsync();
            
            foreach (var product in products)
            {
                product.Id = 0; 
                
                
            }
            
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
        }
    }
} 