using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thuc_hanh_WEB.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        public int UserID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        /// <summary>Pending | Confirmed | Cancelled</summary>
        public string Status { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        public string Note { get; set; }

        /// <summary>COD | BankTransfer | VNPAY</summary>
        public string PaymentMethod { get; set; }

        /// <summary>Unpaid | Paid | Refunded</summary>
        public string PaymentStatus { get; set; }

        /// <summary>Pending | Processing | Shipping | Delivered | Returned</summary>
        public string ShippingStatus { get; set; }

        public string ShippingCode { get; set; }

        public DateTime? DeliveredDate { get; set; }

        // Navigation
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}