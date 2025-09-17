using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TPECOM.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string? Email { get; set; }
        
        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string? Password { get; set; }
        
        [Required(ErrorMessage = "Le prénom est obligatoire")]
        public string? FirstName { get; set; }
        
        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string? LastName { get; set; }
        
        public string FullName => $"{FirstName} {LastName}";
        
        [Required(ErrorMessage = "Le type de compte est obligatoire")]
        public UserType UserType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Informations supplémentaires pour les vendeurs
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyDescription { get; set; }
        
        // Historique des achats pour les clients
        public List<Order> Orders { get; set; } = new List<Order>();
        
        // Historique des ventes pour les vendeurs
        public List<OrderItem> SoldItems { get; set; } = new List<OrderItem>();
        
        // Montant total dépensé (pour les clients)
        public decimal TotalSpent => Orders.Sum(o => o.Total);
        
        // Montant total gagné (pour les vendeurs)
        public decimal TotalEarned => SoldItems.Sum(i => i.Subtotal);
    }
    
    public enum UserType
    {
        Client,
        Vendeur
    }
} 