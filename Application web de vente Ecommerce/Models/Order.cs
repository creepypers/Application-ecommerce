using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TPECOM.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string? BillingAddress { get; set; }

        [Required(ErrorMessage = "L'adresse de livraison est obligatoire")]
        public string? ShippingAddress { get; set; }
        
        public string? TrackingNumber { get; set; }
        
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        
        public decimal ShippingCost { get; set; }
        
        public decimal Tax { get; set; }
        
        public decimal Total => Subtotal + ShippingCost + Tax;
        
        // Référence à la facture
        public Invoice? Invoice { get; set; }

        public string? PaymentIntentId { get; set; }
    }
    
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded
    }
} 