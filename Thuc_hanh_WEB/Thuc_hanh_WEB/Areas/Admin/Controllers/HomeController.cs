using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        public ActionResult Index()
        {
            // Tổng số sách
            int totalBooks = db.Books.Count();

            // Tổng số user (toàn bộ)
            int totalUsers = db.Users.Count();

            // Tổng số đơn hàng
            int totalOrders = db.Orders.Count();

            // Tổng doanh thu = sum(OrderDetail.Quantity * UnitPrice)
            decimal totalRevenue = db.OrderDetails.Sum(od => (decimal?)od.Quantity * od.UnitPrice) ?? 0;

            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;

            // ============ DỮ LIỆU CHO BIỂU ĐỒ ============

            // 1. Doanh thu theo tháng (12 tháng gần nhất)

            // Fill đủ 12 tháng nếu thiếu
            var revenueData = new List<decimal>();
            var labels = new List<string>();
            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                labels.Add(date.ToString("MM/yyyy"));
                var monthRevenue = db.Orders
                    .Where(o => o.OrderDate.Month == date.Month && o.OrderDate.Year == date.Year)
                    .Sum(o => (decimal?)db.OrderDetails.Where(od => od.OrderID == o.OrderID).Sum(od => od.Quantity * od.UnitPrice)) ?? 0;
                revenueData.Add(monthRevenue);
            }

            ViewBag.RevenueLabels = new JavaScriptSerializer().Serialize(labels);
            ViewBag.RevenueData = new JavaScriptSerializer().Serialize(revenueData.Select(r => (double)r).ToList());

            // 2. Số đơn hàng theo tháng
            var orderCountData = new List<int>();
            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var count = db.Orders.Count(o => o.OrderDate.Month == date.Month && o.OrderDate.Year == date.Year);
                orderCountData.Add(count);
            }
            ViewBag.OrderLabels = new JavaScriptSerializer().Serialize(labels);
            ViewBag.OrderCountData = new JavaScriptSerializer().Serialize(orderCountData);

            // 3. Phân bổ sách theo danh mục
            var categoryData = db.Categories
                .Select(c => new {
                    CategoryName = c.CategoryName,
                    Count = db.Books.Count(b => b.CategoryID == c.CategoryID)
                })
                .Where(x => x.Count > 0)
                .ToList();

            ViewBag.CategoryLabels = new JavaScriptSerializer().Serialize(categoryData.Select(x => x.CategoryName).ToList());
            ViewBag.CategoryCountData = new JavaScriptSerializer().Serialize(categoryData.Select(x => x.Count).ToList());

            // 4. Top 5 sách bán chạy
            var topBooks = db.OrderDetails
                .GroupBy(od => od.BookID)
                .Select(g => new {
                    BookID = g.Key,
                    BookTitle = db.Books.FirstOrDefault(b => b.BookID == g.Key).Title,
                    TotalSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            ViewBag.TopBookTitles = new JavaScriptSerializer().Serialize(topBooks.Select(x => x.BookTitle).ToList());
            ViewBag.TopBookSold = new JavaScriptSerializer().Serialize(topBooks.Select(x => x.TotalSold).ToList());

            // 5. Đơn hàng gần đây
            var recentOrders = db.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList()
                .Select(o => new {
                    o.OrderID,
                    CustomerName = o.FullName,
                    OrderDate = o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    o.TotalAmount,
                    StatusClass = o.Status == "Cancelled"
                        ? "bg-danger"
                        : (o.ShippingStatus == "Delivered"
                            ? "bg-success"
                            : (o.Status == "Pending" ? "bg-warning" : "bg-info"))
                })
                .ToList();
            ViewBag.RecentOrders = recentOrders;

            // 6. Tổng quan nhanh
            var today = DateTime.Today;
            ViewBag.TodayOrders = db.Orders.Count(o => o.OrderDate.Year == today.Year && o.OrderDate.Month == today.Month && o.OrderDate.Day == today.Day);
            ViewBag.MonthOrders = db.Orders.Count(o => o.OrderDate.Month == today.Month && o.OrderDate.Year == today.Year);
            ViewBag.PendingOrders = db.Orders.Count(o => o.Status == "Pending");
            ViewBag.CompletedOrders = db.Orders.Count(o => o.ShippingStatus == "Delivered");

            // 7. Doanh thu hôm nay/tháng
            ViewBag.TodayRevenue = db.Orders
                .Where(o => o.OrderDate.Year == today.Year && o.OrderDate.Month == today.Month && o.OrderDate.Day == today.Day)
                .Sum(o => (decimal?)db.OrderDetails.Where(od => od.OrderID == o.OrderID).Sum(od => od.Quantity * od.UnitPrice)) ?? 0;

            ViewBag.MonthRevenue = db.Orders
                .Where(o => o.OrderDate.Month == today.Month && o.OrderDate.Year == today.Year)
                .Sum(o => (decimal?)db.OrderDetails.Where(od => od.OrderID == o.OrderID).Sum(od => od.Quantity * od.UnitPrice)) ?? 0;

            return View();
        }

    }
}
