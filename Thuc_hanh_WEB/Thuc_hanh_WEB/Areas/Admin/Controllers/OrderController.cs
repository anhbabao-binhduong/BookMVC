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
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = orderId });
        }
    }
}