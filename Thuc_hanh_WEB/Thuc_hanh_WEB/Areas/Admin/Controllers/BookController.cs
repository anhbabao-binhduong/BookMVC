using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;
using System.IO;
using System.Web;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class BookController : BaseController
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        // GET: Admin/Book
        public ActionResult Index()
        {
            var books = db.Books.Include("Author").Include("Category").Include("Publisher").ToList();
            return View(books);
        }

        // GET: Admin/Book/Details/5
        public ActionResult Details(int id)
        {
            var book = db.Books.Include("Author").Include("Category").Include("Publisher").FirstOrDefault(b => b.BookID == id);
            if (book == null) return HttpNotFound();
            return View(book);
        }

        // GET: Admin/Book/Create
        public ActionResult Create()
        {
            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "Name");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "Name");
            return View();
        }

        // POST: Admin/Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Book book, HttpPostedFileBase CoverImageFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh bìa
                if (CoverImageFile != null && CoverImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(CoverImageFile.FileName);
                    string uniqueName = Path.GetFileNameWithoutExtension(fileName) + "_" + System.Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(fileName);
                    string path = Server.MapPath("~/Content/images/");
                    Directory.CreateDirectory(path);
                    string fullPath = Path.Combine(path, uniqueName);
                    CoverImageFile.SaveAs(fullPath);
                    book.CoverImage = "/Content/images/" + uniqueName;
                }

                book.CreatedAt = System.DateTime.Now;
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "Name", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "Name", book.PublisherID);
            return View(book);
        }

        // GET: Admin/Book/Edit/5
        public ActionResult Edit(int id)
        {
            var book = db.Books.Find(id);
            if (book == null) return HttpNotFound();

            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "Name", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "Name", book.PublisherID);
            return View(book);
        }

        // POST: Admin/Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Book book, HttpPostedFileBase CoverImageFile)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Books.Find(book.BookID);
                if (existing == null) return HttpNotFound();

                if (CoverImageFile != null && CoverImageFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(CoverImageFile.FileName);
                    string uniqueName = Path.GetFileNameWithoutExtension(fileName) + "_" + System.Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(fileName);
                    string path = Server.MapPath("~/Content/images/");
                    Directory.CreateDirectory(path);
                    string fullPath = Path.Combine(path, uniqueName);
                    CoverImageFile.SaveAs(fullPath);
                    existing.CoverImage = "/Content/images/" + uniqueName;
                }

                existing.Title = book.Title;
                existing.AuthorID = book.AuthorID;
                existing.Price = book.Price;
                existing.Stock = book.Stock;
                existing.ISBN = book.ISBN;
                existing.Description = book.Description;
                existing.CategoryID = book.CategoryID;
                existing.PublisherID = book.PublisherID;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AuthorID = new SelectList(db.Authors, "AuthorID", "Name", book.AuthorID);
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "Name", book.PublisherID);
            return View(book);
        }

        // GET: Admin/Book/Delete/5
        public ActionResult Delete(int id)
        {
            var book = db.Books.Find(id);
            if (book == null) return HttpNotFound();
            return View(book);
        }

        // POST: Admin/Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}