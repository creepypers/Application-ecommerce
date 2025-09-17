using System.ComponentModel.DataAnnotations;

namespace TPECOM.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Variant { get; set; }
        
        public decimal Subtotal => Price * Quantity;
    }
} 