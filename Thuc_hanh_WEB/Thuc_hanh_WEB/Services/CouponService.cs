using System;
using System.Linq;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Services
{
    /// <summary>
    /// Kết quả validate mã coupon
    /// </summary>
    public class CouponResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public decimal Discount { get; set; }   // Số tiền thực tế được giảm
        public string DiscountLabel { get; set; }   // "30%" hoặc "50.000đ"
    }

    public static class CouponService
    {
        /// <summary>
        /// Validate mã từ DB và tính số tiền giảm.
        /// Tham số incrementUsage = true khi checkout thực sự (tăng UsedCount).
        /// </summary>
        public static CouponResult Apply(string code, decimal subtotal,
                                         bool incrementUsage = false)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Fail("Vui lòng nhập mã giảm giá.");

            code = code.Trim().ToUpper();

            using (var db = new BookStoreDBContext())
            {
                var coupon = db.Coupons
                    .FirstOrDefault(c => c.Code.ToUpper() == code);

                if (coupon == null)
                    return Fail($"Mã \"{code}\" không tồn tại.");

                if (!coupon.IsActive)
                    return Fail($"Mã \"{code}\" đã bị vô hiệu hóa.");

                if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.Now)
                    return Fail($"Mã \"{code}\" đã hết hạn vào " +
                                coupon.ExpiryDate.Value.ToString("dd/MM/yyyy") + ".");

                if (coupon.MaxUsage.HasValue && coupon.UsedCount >= coupon.MaxUsage.Value)
                    return Fail($"Mã \"{code}\" đã hết lượt sử dụng.");

                if (subtotal < coupon.MinOrderAmount)
                    return Fail($"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0}đ " +
                                $"để dùng mã này (còn thiếu " +
                                $"{(coupon.MinOrderAmount - subtotal):N0}đ).");

                // Tính số tiền giảm
                decimal discount;
                if (coupon.DiscountType == "Percent")
                    discount = Math.Round(subtotal * coupon.DiscountValue / 100m);
                else
                    discount = Math.Min(coupon.DiscountValue, subtotal);

                // Tăng UsedCount khi checkout
                if (incrementUsage)
                {
                    coupon.UsedCount++;
                    db.SaveChanges();
                }

                return new CouponResult
                {
                    IsValid = true,
                    Discount = discount,
                    DiscountLabel = coupon.DiscountLabel,
                    Message = coupon.DiscountType == "Percent"
                        ? $"Giảm {coupon.DiscountValue}% — tiết kiệm {discount:N0}đ!"
                        : $"Giảm {coupon.DiscountValue:N0}đ trực tiếp!"
                };
            }
        }

        private static CouponResult Fail(string msg) =>
            new CouponResult { IsValid = false, Message = msg };
    }
}
