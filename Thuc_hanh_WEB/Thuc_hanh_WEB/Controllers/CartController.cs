using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;
using Thuc_hanh_WEB.Services;
namespace Thuc_hanh_WEB.Controllers
{
    public class CartController : Controller
    {
    private BookStoreDBContext db = new BookStoreDBContext();

    // ===== LẤY GIỎ HÀNG TỪ SESSION =====
    private List<CartItem> GetCart()
    {
        var cart = Session["CART"] as List<CartItem>;
        if (cart == null)
        {
            cart = new List<CartItem>();
            Session["CART"] = cart;
        }
        return cart;
    }

    // ===== COUPON HELPER — đổ dữ liệu coupon vào ViewBag =====
    private void LoadCouponViewBag(decimal subtotal)
    {
        var code = Session["CouponCode"] as string;

        if (!string.IsNullOrEmpty(code))
        {
            var result = CouponService.Apply(code, subtotal);

            if (result.IsValid)
            {
                ViewBag.CouponValid = true;
                ViewBag.CouponCode = code;
                ViewBag.CouponMessage = result.Message;
                ViewBag.Discount = result.Discount;
                Session["CouponDiscount"] = result.Discount;
                return;
            }

            // Coupon không còn valid (vd: xóa item làm subtotal < min) → clear
            Session.Remove("CouponCode");
            Session.Remove("CouponDiscount");
        }

        ViewBag.CouponValid = false;
        ViewBag.CouponCode = "";
        ViewBag.CouponMessage = "";
        ViewBag.Discount = 0m;
    }

    // ===== THÊM VÀO GIỎ =====
    [HttpPost]
    public ActionResult AddToCart(int id)
    {
        if (Session["UserID"] == null)
        {
            return Json(new { success = false });
        }

        int userId = (int)Session["UserID"];

        var item = db.ShoppingCarts
            .FirstOrDefault(c => c.BookID == id && c.UserID == userId);

        if (item == null)
        {
            db.ShoppingCarts.Add(new ShoppingCart
            {
                UserID = userId,
                BookID = id,
                Quantity = 1,
                AddedAt = DateTime.Now
            });
        }
        else
        {
            item.Quantity++;
        }

        db.SaveChanges();

        int count = db.ShoppingCarts
            .Where(c => c.UserID == userId)
            .Sum(c => c.Quantity);

        return Json(new { success = true, count = count });
    }

    // ===== XEM GIỎ HÀNG =====
    public ActionResult Index()
    {
        if (Session["UserID"] == null)
        {
            return RedirectToAction("Login", "Account",
                new { returnUrl = Url.Action("Index", "Cart") });
        }

        int userId = (int)Session["UserID"];

        // ===== MUA NGAY =====
        if (Session["BUY_NOW"] != null)
        {
            var buyItem = Session["BUY_NOW"] as CartItem;

            if (buyItem != null)
            {
                Session["SelectedBookIDs"] = new List<int> { buyItem.BookID };

                decimal buySubtotal = buyItem.Price * buyItem.Quantity;
                LoadCouponViewBag(buySubtotal);

                return View(new List<CartItem> { buyItem });
            }

            Session.Remove("BUY_NOW");
        }

        // ===== GIỎ HÀNG DB =====
        var cart = db.ShoppingCarts
            .Where(c => c.UserID == userId)
            .Select(c => new CartItem
            {
                BookID = c.BookID,
                Title = c.Book.Title,
                Image = c.Book.CoverImage,
                Price = c.Book.Price,
                Quantity = c.Quantity
            })
            .ToList();

        decimal subtotal = cart.Sum(c => c.Price * c.Quantity);
        LoadCouponViewBag(subtotal);

        return View(cart);
    }

    // ===== TĂNG SỐ LƯỢNG =====
    public ActionResult Increase(int id)
    {
        int userId = (int)Session["UserID"];

        var item = db.ShoppingCarts
            .FirstOrDefault(c => c.BookID == id && c.UserID == userId);

        if (item != null)
        {
            item.Quantity++;
            db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // ===== GIẢM SỐ LƯỢNG =====
    public ActionResult Decrease(int id)
    {
        int userId = (int)Session["UserID"];

        var item = db.ShoppingCarts
            .FirstOrDefault(c => c.BookID == id && c.UserID == userId);

        if (item != null)
        {
            item.Quantity--;

            if (item.Quantity <= 0)
                db.ShoppingCarts.Remove(item);

            db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // ===== XÓA SẢN PHẨM =====
    public ActionResult Remove(int id)
    {
        int userId = (int)Session["UserID"];

        var item = db.ShoppingCarts
            .FirstOrDefault(c => c.BookID == id && c.UserID == userId);

        if (item != null)
        {
            db.ShoppingCarts.Remove(item);
            db.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // ===== MUA NGAY =====
    [HttpPost]
    public ActionResult BuyNow(int id)
    {
        if (Session["UserID"] == null)
            return Json(new { success = false });

        var book = db.Books.Find(id);
        if (book == null)
            return Json(new { success = false });

        var buyNowItem = new CartItem
        {
            BookID = book.BookID,
            Title = book.Title,
            Image = book.CoverImage,
            Price = book.Price,
            Quantity = 1
        };

        Session["BUY_NOW"] = buyNowItem;

        return Json(new { success = true });
    }

    // ===== ĐẾM GIỎ HÀNG =====
    public int GetCartCount()
    {
        if (Session["UserID"] == null)
            return 0;

        int userId = (int)Session["UserID"];

        return db.ShoppingCarts
                 .Where(c => c.UserID == userId)
                 .Sum(c => (int?)c.Quantity) ?? 0;
    }

    // ===== ÁP MÃ KHUYẾN MÃI =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ApplyCoupon(string couponCode, List<int> selectedIds)
    {
        if (Session["UserID"] == null)
            return RedirectToAction("Login", "Account");

        int userId = (int)Session["UserID"];

        var cart = db.ShoppingCarts
            .Where(c => c.UserID == userId)
            .Select(c => new CartItem
            {
                BookID = c.BookID,
                Title = c.Book.Title,
                Image = c.Book.CoverImage,
                Price = c.Book.Price,
                Quantity = c.Quantity
            })
            .ToList();

        // Lưu danh sách đã chọn vào Session
        Session["SelectedBookIDs"] = selectedIds ?? new List<int>();

        // Nếu không chọn gì → thông báo lỗi nhưng vẫn giữ danh sách rỗng
        if (selectedIds == null || !selectedIds.Any())
        {
            TempData["CouponMessage"] = "Vui lòng chọn sản phẩm để áp dụng mã giảm giá!";
            TempData["CouponValid"] = false;
            Session.Remove("CouponCode");
            Session.Remove("CouponDiscount");
            return RedirectToAction("Index");
        }

        // ✅ Tính theo sản phẩm đã chọn
        decimal subtotal = cart
            .Where(c => selectedIds.Contains(c.BookID))
            .Sum(c => c.Price * c.Quantity);

        var result = CouponService.Apply(couponCode, subtotal);

        if (result.IsValid)
        {
            Session["CouponCode"] = couponCode.Trim().ToUpper();
            Session["CouponDiscount"] = result.Discount;
            TempData["CouponMessage"] = result.Message;
            TempData["CouponValid"] = true;
        }
        else
        {
            Session.Remove("CouponCode");
            Session.Remove("CouponDiscount");
            TempData["CouponMessage"] = result.Message;
            TempData["CouponValid"] = false;
        }

        return RedirectToAction("Index");
    }

    // ===== THANH TOÁN VỚI SẢN PHẨM ĐÃ CHỌN =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult CheckoutSelected(List<int> selectedIds)
    {
        if (Session["UserID"] == null)
            return RedirectToAction("Login", "Account");

        if (selectedIds == null || !selectedIds.Any())
        {
            TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
            return RedirectToAction("Index");
        }

        // Lưu danh sách BookID được chọn vào Session để OrderController dùng
        Session["SelectedBookIDs"] = selectedIds;

        return RedirectToAction("Checkout", "Order");
    }
    // ===== LƯU DANH SÁCH ID ĐÃ CHỌN =====
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult SaveSelectedIds(List<int> selectedIds)
    {
        if (Session["UserID"] != null)
        {
            Session["SelectedBookIDs"] = selectedIds ?? new List<int>();
        }
        return Json(new { success = true });
    }
    // ===== XÓA MÃ KHUYẾN MÃI =====
    public ActionResult RemoveCoupon()
    {
        Session.Remove("CouponCode");
        Session.Remove("CouponDiscount");
        return RedirectToAction("Index");
    }
}
}