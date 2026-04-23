using System;
using System.Collections.Generic;
using System.Linq; // Thêm dòng này để dùng Sum()
using Thuc_hanh_WEB.Models.ViewModels;

namespace Thuc_hanh_WEB.Models.ViewModels
{
    public class OrderListViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string ShippingStatus { get; set; }
        public string ShippingCode { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public List<OrderItemViewModel> Items { get; set; }

        public int TotalItems => Items?.Sum(i => i.Quantity) ?? 0; // Đã có using System.Linq

        public string OrderStatusDisplay
        {
            get
            {
                // Sửa switch expression thành switch statement để tương thích C# 7.3
                switch (Status)
                {
                    case "Pending":
                        return "Chờ xác nhận";
                    case "Confirmed":
                        return "Đã xác nhận";
                    case "Cancelled":
                        return "Đã hủy";
                    default:
                        return Status;
                }
            }
        }
    }
}