using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Controllers
{
    public class BookController : Controller
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        public ActionResult Index()
        {
            var books = db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .ToList();

            return View(books);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("Index", "Home");

            var book = db.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .FirstOrDefault(b => b.BookID == id);

            if (book == null)
                return RedirectToAction("Index", "Home");

            ViewBag.RelatedBooks = db.Books
                .Where(b => b.CategoryID == book.CategoryID && b.BookID != id)
                .Take(4)
                .ToList();

            return View(book);
        }

        // ⭐ API Search realtime - FIXED VERSION
        public JsonResult Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            try
            {
                var result = db.Books
                    .Include(b => b.Author) // Include Author để tránh lỗi null
                    .Where(b => b.Title.Contains(keyword) ||
                               (b.Author != null && b.Author.Name.Contains(keyword)))
                    .Select(b => new
                    {
                        b.BookID,
                        b.Title,
                        CoverImage = b.CoverImage ?? "default.jpg", // Xử lý null
                        AuthorName = b.Author != null ? b.Author.Name : "Không có tác giả"
                    })
                    .Take(8) // Tăng lên 8 kết quả
                    .ToList();

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                // Log lỗi nếu cần
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}