using System.Collections.Generic;

namespace TPECOM.Models
{
    public class Checkout
    {
        public List<CartItem> CartItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
        public string PublicKey { get; set; }
    }
} 