// Models/ViewModels/MyOrdersViewModel.cs (cập nhật)
using System.Collections.Generic;

namespace Thuc_hanh_WEB.Models.ViewModels
{
    public class MyOrdersViewModel
    {
        public List<OrderListViewModel> Orders { get; set; }
        public string Filter { get; set; }

        // Statistics
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ShippingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
} 