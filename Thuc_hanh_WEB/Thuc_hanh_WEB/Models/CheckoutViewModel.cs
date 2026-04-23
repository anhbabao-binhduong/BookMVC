using System.Collections.Generic;

namespace Thuc_hanh_WEB.Models
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public string PaymentMethod { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}