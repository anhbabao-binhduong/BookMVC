using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thuc_hanh_WEB.Models
{
    [Table("Coupons")]
    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }

        // ── MÃ CODE ─────────────────────────
        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá")]
        [StringLength(50)]
        public string Code { get; set; }

        // Percent | Fixed
        [Required]
        [StringLength(10)]
        public string DiscountType { get; set; }

        // GIÁ TRỊ GIẢM
        [Required]
        [Column(TypeName = "decimal")]
        public decimal DiscountValue { get; set; }

        // ĐƠN TỐI THIỂU
        [Column(TypeName = "decimal")]
        public decimal MinOrderAmount { get; set; } = 0;

        // HẾT HẠN
        public DateTime? ExpiryDate { get; set; }

        // GIỚI HẠN LƯỢT DÙNG
        public int? MaxUsage { get; set; }

        public int UsedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ===== KHÔNG LƯU DB =====
        [NotMapped]
        public bool IsCurrentlyValid =>
            IsActive &&
            (ExpiryDate == null || ExpiryDate > DateTime.Now) &&
            (MaxUsage == null || UsedCount < MaxUsage);

        [NotMapped]
        public string DiscountLabel =>
            DiscountType == "Percent"
                ? $"{DiscountValue}%"
                : $"{DiscountValue:N0}đ";
    }
}