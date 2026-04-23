using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class CouponController : BaseController
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        // GET: Admin/Coupon
        public ActionResult Index()
        {
            return View(db.Coupons.ToList());
        }

        // GET: Admin/Coupon/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                coupon.CreatedAt = System.DateTime.Now;
                coupon.UsedCount = 0;
                db.Coupons.Add(coupon);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        // GET: Admin/Coupon/Edit/5
        public ActionResult Edit(int id)
        {
            var coupon = db.Coupons.Find(id);
            if (coupon == null) return HttpNotFound();
            return View(coupon);
        }

        // POST: Admin/Coupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                db.Entry(coupon).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(coupon);
        }

        // GET: Admin/Coupon/Delete/5
        public ActionResult Delete(int id)
        {
            var coupon = db.Coupons.Find(id);
            if (coupon == null) return HttpNotFound();
            return View(coupon);
        }

        // POST: Admin/Coupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var coupon = db.Coupons.Find(id);
            db.Coupons.Remove(coupon);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}