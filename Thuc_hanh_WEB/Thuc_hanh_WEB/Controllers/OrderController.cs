using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Helpers;
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
        var user = db.Users.Find(userId);

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

        // ── VNPAY: redirect sang cổng thanh toán ─────────────────────────────
        // Với VNPAY chỉ gửi email sau khi thanh toán thành công ở action VnPayReturn
        if (order.PaymentMethod == "VNPAY")
        {
            string vnpUrl = ConfigurationManager.AppSettings["vnp_Url"];
            string vnpHashKey = ConfigurationManager.AppSettings["vnp_HashSecret"];
            string vnpTmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
            string returnUrl = Request.Url.GetLeftPart(UriPartial.Authority)
                               + Url.Action("VnPayReturn", "Order");

            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnpTmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(total * 100)).ToString());
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_BankCode", "");
            vnpay.AddRequestData("vnp_TxnRef", order.OrderID.ToString());
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang #" + order.OrderID);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_IpAddr", Request.UserHostAddress ?? "127.0.0.1");

            TimeZoneInfo vietnamZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamZone);
            vnpay.AddRequestData("vnp_CreateDate", vietnamTime.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_ExpireDate", vietnamTime.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            string paymentUrl = vnpay.CreateRequestUrl(vnpUrl, vnpHashKey);
            return Redirect(paymentUrl);
        }

        // ── Gửi email xác nhận đơn hàng (COD/BankTransfer...) ───────────────
        try
        {
            var toEmail = user?.Email;
            if (!string.IsNullOrWhiteSpace(toEmail))
            {
                // Load lại chi tiết đơn + book để render email (tránh null OrderDetails/Book)
                order.OrderDetails = db.OrderDetails
                    .Include(od => od.Book)
                    .Where(od => od.OrderID == order.OrderID)
                    .ToList();

                EmailHelper.SendOrderConfirmation(toEmail, order);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("SendOrderConfirmation failed. OrderID={0}. Error={1}", order.OrderID, ex);
            // Không throw để tránh làm fail luồng đặt hàng
        }

        TempData["OrderSuccess"] = true;
        TempData["OrderID"] = order.OrderID;
        TempData["OrderTotal"] = total;
        TempData["PaymentMethod"] = model.PaymentMethod;
        TempData["FullName"] = model.FullName;

        return RedirectToAction("OrderSuccess");
    }

    // ================================================================
    // GET /Order/VnPayReturn
    // ================================================================
    [HttpGet]
    public ActionResult VnPayReturn()
    {
        string hashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];
        bool isValid      = VnPayLibrary.ValidateSignature(Request.QueryString, hashSecret);

        string responseCode = Request.QueryString["vnp_ResponseCode"];
        string txnRef       = Request.QueryString["vnp_TxnRef"];  // OrderID
        string transactionNo= Request.QueryString["vnp_TransactionNo"];

        if (!isValid)
        {
            TempData["VnPayError"] = "Chữ ký không hợp lệ. Vui lòng liên hệ hỗ trợ.";
            return RedirectToAction("VnPayFail");
        }

        if (!int.TryParse(txnRef, out int orderId))
        {
            TempData["VnPayError"] = "Mã đơn hàng không hợp lệ.";
            return RedirectToAction("VnPayFail");
        }

        var order = db.Orders.Find(orderId);
        if (order == null)
        {
            TempData["VnPayError"] = "Không tìm thấy đơn hàng.";
            return RedirectToAction("VnPayFail");
        }

        if (responseCode == "00")
        {
            // Thanh toán thành công
            order.PaymentStatus  = "Paid";
            order.Status         = "Pending";
            db.SaveChanges();

            var paidUser = db.Users.Find(order.UserID);
            if (paidUser != null && !string.IsNullOrWhiteSpace(paidUser.Email))
            {
                try
                {
                    order.OrderDetails = db.OrderDetails
                        .Include(od => od.Book)
                        .Where(od => od.OrderID == order.OrderID)
                        .ToList();

                    EmailHelper.SendOrderConfirmation(paidUser.Email, order);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(
                        "SendOrderConfirmation failed (VNPAY). OrderID={0}. Error={1}",
                        order.OrderID,
                        ex
                    );
                }
            }

            TempData["OrderSuccess"]  = true;
            TempData["OrderID"]       = order.OrderID;
            TempData["OrderTotal"]    = order.TotalAmount;
            TempData["PaymentMethod"] = "VNPAY";
            TempData["FullName"]      = order.FullName;
            TempData["TransactionNo"] = transactionNo;

            return RedirectToAction("OrderSuccess");
        }
        else
        {
            // Thanh toán thất bại / bị hủy
            order.Status        = "Cancelled";
            order.PaymentStatus = "Unpaid";
            db.SaveChanges();

            string errorMsg = "Thanh toán thất bại hoặc bị hủy.";
            switch (responseCode)
            {
                case "24": errorMsg = "Bạn đã hủy giao dịch thanh toán."; break;
                case "51": errorMsg = "Tài khoản của bạn không đủ số dư để thực hiện giao dịch."; break;
                case "12": errorMsg = "Thẻ/Tài khoản của bạn bị khóa."; break;
                case "09": errorMsg = "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking."; break;
                case "10": errorMsg = "Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần."; break;
                case "11": errorMsg = "Đã hết hạn chờ thanh toán."; break;
                case "13": errorMsg = "Nhập sai mật khẩu xác thực giao dịch (OTP)."; break;
                case "65": errorMsg = "Tài khoản đã vượt quá hạn mức giao dịch trong ngày."; break;
                case "75": errorMsg = "Ngân hàng thanh toán đang bảo trì."; break;
                case "79": errorMsg = "Nhập sai mật khẩu thanh toán quá số lần quy định."; break;
                default:   errorMsg = $"Lỗi thanh toán từ ngân hàng (Mã lỗi: {responseCode})."; break;
            }

            TempData["VnPayError"]      = errorMsg;
            TempData["VnPayOrderID"]    = orderId;
            return RedirectToAction("VnPayFail");
        }
    }

    // ================================================================
    // GET /Order/VnPayFail
    // ================================================================
    [HttpGet]
    public ActionResult VnPayFail()
    {
        ViewBag.Error   = TempData["VnPayError"]   as string ?? "Thanh toán thất bại.";
        ViewBag.OrderID = TempData["VnPayOrderID"];
        return View();
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
        var buyNowItem = Session["BUY_NOW"] as CartItem;

        if (selectedIds != null && selectedIds.Any())
        {
            var selectedItems = db.ShoppingCarts
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

            if (selectedItems.Any())
                return selectedItems;

            if (buyNowItem != null && selectedIds.Contains(buyNowItem.BookID))
                return new List<CartItem> { buyNowItem };
        }

        if (buyNowItem != null)
            return new List<CartItem> { buyNowItem };

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
