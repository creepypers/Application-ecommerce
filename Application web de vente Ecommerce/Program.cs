using Microsoft.EntityFrameworkCore;
using TPECOM.Data;
using TPECOM.Services;

namespace TPECOM
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configuration de la session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Ajouter la connexion à la base de données
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Enregistrer les services
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<DummyJsonService>();
            builder.Services.AddScoped<StripeService>();
            builder.Services.AddScoped<DbInitializer>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            // Activer la session
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Initialiser la base de données
            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                initializer.InitializeAsync().GetAwaiter().GetResult();
            }

            app.Run();
        }
    }
}
