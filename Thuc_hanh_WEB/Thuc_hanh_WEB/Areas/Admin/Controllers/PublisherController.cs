using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class PublisherController : BaseController
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        public ActionResult Index()
        {
            return View(db.Publishers.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                db.Publishers.Add(publisher);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(publisher);
        }

        public ActionResult Edit(int id)
        {
            var publisher = db.Publishers.Find(id);
            if (publisher == null) return HttpNotFound();
            return View(publisher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(publisher).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(publisher);
        }

        public ActionResult Delete(int id)
        {
            var publisher = db.Publishers.Find(id);
            if (publisher == null) return HttpNotFound();
            return View(publisher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var publisher = db.Publishers.Find(id);
            db.Publishers.Remove(publisher);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}