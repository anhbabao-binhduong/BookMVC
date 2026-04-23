// Models/ViewModels/OrderItemViewModel.cs
namespace Thuc_hanh_WEB.Models.ViewModels
{
    public class OrderItemViewModel
    {
        public int BookID { get; set; }
        public string BookTitle { get; set; }
        public string BookImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => UnitPrice * Quantity;
    }
}