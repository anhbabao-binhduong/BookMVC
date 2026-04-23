using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class AuthorController : BaseController
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        // GET: Admin/Author
        public ActionResult Index()
        {
            return View(db.Authors.ToList());
        }

        // GET: Admin/Author/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Author/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Author author)
        {
            if (ModelState.IsValid)
            {
                db.Authors.Add(author);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(author);
        }

        // GET: Admin/Author/Edit/5
        public ActionResult Edit(int id)
        {
            var author = db.Authors.Find(id);
            if (author == null) return HttpNotFound();
            return View(author);
        }

        // POST: Admin/Author/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Author author)
        {
            if (ModelState.IsValid)
            {
                db.Entry(author).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(author);
        }

        // GET: Admin/Author/Delete/5
        public ActionResult Delete(int id)
        {
            var author = db.Authors.Find(id);
            if (author == null) return HttpNotFound();
            return View(author);
        }

        // POST: Admin/Author/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var author = db.Authors.Find(id);
            db.Authors.Remove(author);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}