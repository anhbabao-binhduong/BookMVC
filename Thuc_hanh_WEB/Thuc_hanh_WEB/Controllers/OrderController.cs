using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;
using Thuc_hanh_WEB.Models.ViewModels;
namespace Thuc_hanh_WEB.Controllers
{
    public class OrderController : Controller
    {
    private BookStoreDBContext db = new BookStoreDBContext();

    // ================================================================
    // GET /Order/MyOrders
    [HttpGet]
    public ActionResult MyOrders(string filter = "all", int page = 1)
    {
        if (Session["UserID"] == null)
        {
            return RedirectToAction("Login", "Account",
                new { returnUrl = Url.Action("MyOrders", "Order") });
        }

        int userId = (int)Session["UserID"];
        int pageSize = 10;

        // Tạo view model cho danh sách đơn hàng
        var orderViewModels = db.Orders
            .Where(o => o.UserID == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderListViewModel
            {
                OrderID = o.OrderID,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                ShippingStatus = o.ShippingStatus,
                ShippingCode = o.ShippingCode,
                DeliveredDate = o.DeliveredDate,
                FullName = o.FullName,
                Phone = o.Phone,
                Address = o.Address,
                Items = o.OrderDetails.Select(od => new OrderItemViewModel
                {
                    BookID = od.BookID,
                    BookTitle = od.Book.Title,  // Lấy trực tiếp từ Book
                    BookImage = od.Book.CoverImage,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList()
            })
            .ToList();

        // Apply filter
        IEnumerable<OrderListViewModel> filteredOrders = orderViewModels;

        switch (filter?.ToLower())
        {
            case "pending":
                filteredOrders = orderViewModels.Where(o => o.Status == "Pending");
                break;
            case "confirmed":
                filteredOrders = orderViewModels.Where(o => o.Status == "Confirmed");
                break;
            case "cancelled":
                filteredOrders = orderViewModels.Where(o => o.Status == "Cancelled");
                break;
            case "shipping":
                filteredOrders = orderViewModels.Where(o => o.ShippingStatus == "Shipping");
                break;
            case "delivered":
                filteredOrders = orderViewModels.Where(o => o.ShippingStatus == "Delivered");
                break;
            default:
                filter = "all";
                break;
        }

        // Statistics
        var totalSpent = orderViewModels
            .Where(o => o.Status == "Confirmed" || o.ShippingStatus == "Delivered")
            .Sum(o => (decimal?)o.TotalAmount) ?? 0;

        var viewModel = new MyOrdersViewModel
        {
            Filter = filter,
            CurrentPage = page,
            PageSize = pageSize,
            TotalOrders = orderViewModels.Count,
            TotalSpent = totalSpent,
            PendingOrders = orderViewModels.Count(o => o.Status == "Pending"),
            ConfirmedOrders = orderViewModels.Count(o => o.Status == "Confirmed"),
            ShippingOrders = orderViewModels.Count(o => o.ShippingStatus == "Shipping"),
            DeliveredOrders = orderViewModels.Count(o => o.ShippingStatus == "Delivered"),
            CancelledOrders = orderViewModels.Count(o => o.Status == "Cancelled"),
            Orders = filteredOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList()
        };

        viewModel.TotalPages = (int)Math.Ceiling(filteredOrders.Count() / (double)pageSize);

        return View(viewModel);
    }

    // ================================================================
    // GET /Order/OrderDetail/5
    // ================================================================
    [HttpGet]
    public ActionResult OrderDetail(int id)
    {
        if (Session["UserID"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserID"];

        var order = db.Orders
            .Include(o => o.OrderDetails.Select(od => od.Book))
            .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

        if (order == null)
        {
            return HttpNotFound();
        }

        return View(order);
    }

    // ================================================================
    // POST /Order/CancelOrder
    // ================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult CancelOrder(int id)
    {
        try
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại!" });
            }

            int userId = (int)Session["UserID"];

            var order = db.Orders
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            // Chỉ cho phép hủy đơn hàng ở trạng thái Pending
            if (order.Status != "Pending")
            {
                return Json(new
                {
                    success = false,
                    message = "Chỉ có thể hủy đơn hàng đang chờ xác nhận!"
                });
            }

            order.Status = "Cancelled";
            order.ShippingStatus = "Returned";
            db.SaveChanges();

            return Json(new
            {
                success = true,
                message = "Hủy đơn hàng thành công!"
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Có lỗi xảy ra: " + ex.Message
            });
        }
    }

    // ================================================================
    // POST /Order/Reorder
    // ================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public JsonResult Reorder(int id)
    {
        try
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại!" });
            }

            int userId = (int)Session["UserID"];

            var order = db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }

            // Thêm sản phẩm vào giỏ hàng
            var cart = Session["CART"] as List<CartItem> ?? new List<CartItem>();

            foreach (var detail in order.OrderDetails)
            {
                var existingItem = cart.FirstOrDefault(c => c.BookID == detail.BookID);
                if (existingItem != null)
                {
                    existingItem.Quantity += detail.Quantity;
                }
                else
                {
                    var book = db.Books.Find(detail.BookID);
                    cart.Add(new CartItem
                    {
                        BookID = detail.BookID,
                        Title = book?.Title ?? "Sách",
                        Image = book?.CoverImage ?? "default.jpg",
                        Price = detail.UnitPrice,
                        Quantity = detail.Quantity
                    });
                }
            }

            Session["CART"] = cart;

            return Json(new
            {
                success = true,
                message = "Đã thêm sản phẩm vào giỏ hàng!",
                redirectUrl = Url.Action("Index", "Cart")
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                message = "Có lỗi xảy ra: " + ex.Message
            });
        }
    }

    // ================================================================
    // GET /Order/TrackOrder/5
    // ================================================================
    [HttpGet]
    public ActionResult TrackOrder(int id)
    {
        if (Session["UserID"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserID"];

        var order = db.Orders
            .Include(o => o.OrderDetails.Select(od => od.Book))
            .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

        if (order == null)
        {
            return HttpNotFound();
        }

        return View(order);
    }

    // ================================================================
    // GET /Order/Checkout
    // ================================================================
    [HttpGet]
    public ActionResult Checkout()
    {
        if (Session["UserID"] == null)
            return RedirectToAction("Login", "Account",
                new { returnUrl = Url.Action("Checkout", "Order") });

        int userId = (int)Session["UserID"];

        var items = GetCheckoutItems(userId);

        if (items == null || !items.Any())
        {
            TempData["Error"] = "Không có sản phẩm nào để thanh toán.";
            return RedirectToAction("Index", "Cart");
        }

        var user = db.Users.Find(userId);
        var vm = new CheckoutViewModel
        {
            FullName = user?.FullName ?? Session["FullName"]?.ToString(),
            Phone = user?.Phone,
            Address = user?.Address,
            Items = items
        };

        return View(vm);
    }

    // ================================================================
    // POST /Order/PlaceOrder
    // ================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult PlaceOrder(CheckoutViewModel model)
    {
        if (Session["UserID"] == null)
            return RedirectToAction("Login", "Account");

        int userId = (int)Session["UserID"];

        // ── Validate ─────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(model.FullName) ||
            string.IsNullOrWhiteSpace(model.Phone) ||
            string.IsNullOrWhiteSpace(model.Address))
        {
            TempData["Error"] = "Vui lòng điền đầy đủ họ tên, số điện thoại và địa chỉ.";
            return RedirectToAction("Checkout");
        }

        // ── Lấy items từ Session (không tin form) ────────────────
        var items = GetCheckoutItems(userId);
        if (items == null || !items.Any())
        {
            TempData["Error"] = "Phiên đặt hàng đã hết hạn, vui lòng thử lại.";
            return RedirectToAction("Index", "Cart");
        }

        // ── Tính tiền ────────────────────────────────────────────
        decimal subtotal = items.Sum(i => i.Price * i.Quantity);
        decimal discount = Session["CouponDiscount"] != null
                           ? (decimal)Session["CouponDiscount"] : 0m;
        decimal ship = subtotal >= 199000 ? 0m : 30000m;
        decimal total = subtotal - discount + ship;

        // ── Tạo Order ────────────────────────────────────────────
        var order = new Order
        {
            UserID = userId,
            OrderDate = DateTime.Now,
            TotalAmount = total,
            Status = "Pending",
            FullName = model.FullName.Trim(),
            Phone = model.Phone.Trim(),
            Address = model.Address.Trim(),
            Note = model.Note?.Trim(),
            PaymentMethod = model.PaymentMethod ?? "COD",
            PaymentStatus = "Unpaid",
            ShippingStatus = "Pending",
            ShippingCode = null,
            DeliveredDate = null
        };

        db.Orders.Add(order);
        db.SaveChanges(); // lấy OrderID sau khi insert

        // ── Tạo OrderDetails ─────────────────────────────────────
        foreach (var item in items)
        {
            db.OrderDetails.Add(new OrderDetail
            {
                OrderID = order.OrderID,
                BookID = item.BookID,
                Quantity = item.Quantity,
                UnitPrice = item.Price
            });
        }

        // ── Xóa sản phẩm đã đặt khỏi giỏ hàng ──────────────────
        var selectedIds = Session["SelectedBookIDs"] as List<int>;
        if (selectedIds != null && selectedIds.Any())
        {
            var toRemove = db.ShoppingCarts
                .Where(c => c.UserID == userId && selectedIds.Contains(c.BookID));
            db.ShoppingCarts.RemoveRange(toRemove);
        }

        db.SaveChanges();

        // ── Clear session ────────────────────────────────────────
        Session.Remove("SelectedBookIDs");
        Session.Remove("BUY_NOW");
        Session.Remove("CouponCode");
        Session.Remove("CouponDiscount");

        // ── VNPAY: redirect sang cổng thanh toán ─────────────────
        if (model.PaymentMethod == "VNPAY")
        {
            // TODO: return RedirectToAction("VnpayCheckout", new { orderId = order.OrderID });
        }

        TempData["OrderSuccess"] = true;
        TempData["OrderID"] = order.OrderID;
        TempData["OrderTotal"] = total;
        TempData["PaymentMethod"] = model.PaymentMethod;
        TempData["FullName"] = model.FullName;

        return RedirectToAction("OrderSuccess");
    }

    // ================================================================
    // GET /Order/OrderSuccess
    // ================================================================
    [HttpGet]
    public ActionResult OrderSuccess()
    {
        if (TempData["OrderSuccess"] == null)
            return RedirectToAction("Index", "Home");

        ViewBag.OrderID = TempData["OrderID"];
        ViewBag.Total = TempData["OrderTotal"];
        ViewBag.PaymentMethod = TempData["PaymentMethod"];
        ViewBag.FullName = TempData["FullName"];
        return View();
    }

    // ================================================================
    // Helper: lấy items checkout từ session
    // ================================================================
    private List<CartItem> GetCheckoutItems(int userId)
    {
        var selectedIds = Session["SelectedBookIDs"] as List<int>;

        if (selectedIds != null && selectedIds.Any())
        {
            return db.ShoppingCarts
                .Where(c => c.UserID == userId && selectedIds.Contains(c.BookID))
                .Select(c => new CartItem
                {
                    BookID = c.BookID,
                    Title = c.Book.Title,
                    Image = c.Book.CoverImage,
                    Price = c.Book.Price,
                    Quantity = c.Quantity
                })
                .ToList();
        }

        if (Session["BUY_NOW"] != null)
            return new List<CartItem> { Session["BUY_NOW"] as CartItem };

        return null;
    }

    // ================================================================
    // Dispose
    // ================================================================
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }
}
}