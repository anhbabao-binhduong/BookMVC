namespace Thuc_hanh_WEB.Models.ViewModels
{
    public class AdminRecentOrderViewModel
    {
        public int OrderID { get; set; }

        public string CustomerName { get; set; }

        public string OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string StatusClass { get; set; }
    }
}