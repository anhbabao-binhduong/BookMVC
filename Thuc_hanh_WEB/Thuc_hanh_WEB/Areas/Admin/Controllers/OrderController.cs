using System;
using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class OrderController : BaseController
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        public ActionResult Index()
        {
            var orders = db.Orders.Include("User").OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        public ActionResult Details(int id)
        {
            var order = db.Orders.Include("User").FirstOrDefault(o => o.OrderID == id);
            if (order == null) return HttpNotFound();
            var details = db.OrderDetails.Include("Book").Where(od => od.OrderID == id).ToList();
            ViewBag.OrderDetails = details;
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateStatus(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            var nextStatus = (status ?? string.Empty).Trim();

            switch (nextStatus)
            {
                case "Pending":
                    order.Status = "Pending";
                    if (string.IsNullOrEmpty(order.ShippingStatus) || order.ShippingStatus == "Returned")
                    {
                        order.ShippingStatus = "Pending";
                    }
                    order.DeliveredDate = null;
                    break;

                case "Processing":
                    order.Status = "Completed";
                    order.ShippingStatus = "Processing";
                    order.DeliveredDate = null;
                    break;

                case "Shipping":
                    order.Status = "Completed";
                    order.ShippingStatus = "Shipping";
                    order.DeliveredDate = null;
                    break;

                case "Delivered":
                    order.Status = "Completed";
                    order.ShippingStatus = "Delivered";
                    if (!order.DeliveredDate.HasValue)
                    {
                        order.DeliveredDate = DateTime.Now;
                    }
                    break;

                case "Cancelled":
                    order.Status = "Cancelled";
                    if (order.ShippingStatus == "Delivered")
                    {
                        order.ShippingStatus = "Returned";
                    }
                    order.DeliveredDate = null;
                    break;

                default:
                    return RedirectToAction("Details", new { id = orderId });
            }

            db.SaveChanges();
            return RedirectToAction("Details", new { id = orderId });
        }
    }
}