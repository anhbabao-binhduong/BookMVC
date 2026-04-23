using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class CategoryController : BaseController
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        public ActionResult Index()
        {
            return View(db.Categories.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public ActionResult Edit(int id)
        {
            var c = db.Categories.Find(id);
            if (c == null) return HttpNotFound();
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public ActionResult Delete(int id)
        {
            var c = db.Categories.Find(id);
            if (c == null) return HttpNotFound();
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var c = db.Categories.Find(id);
            db.Categories.Remove(c);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}