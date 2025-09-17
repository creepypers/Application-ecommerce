using System;
using System.ComponentModel.DataAnnotations;

namespace TPECOM.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public string InvoiceNumber { get; set; }
        
        public DateTime IssueDate { get; set; } = DateTime.Now;
        
        public DateTime DueDate { get; set; }
        
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Paid;
        
        // Référence à la commande associée
        public Order Order { get; set; }
        
        // Méthode pour générer un numéro de facture unique
        public static string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
    
    public enum InvoiceStatus
    {
        Pending,
        Paid,
        Overdue,
        Cancelled
    }
} 