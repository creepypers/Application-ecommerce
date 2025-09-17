using System;
using System.ComponentModel.DataAnnotations;

namespace TPECOM.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        
        public int ProductId { get; set; }
        
        public int? SellerId { get; set; }
        
        public string ProductName { get; set; }
        
        public string ProductImageUrl { get; set; }
        
        public decimal Price { get; set; }
        
        public int Quantity { get; set; }
        
        public string? Variant { get; set; }
        
        public decimal Subtotal => Price * Quantity;
        
        public OrderStatus Status { get; set; }
    }
} 