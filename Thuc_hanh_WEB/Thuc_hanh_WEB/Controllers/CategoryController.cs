using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Controllers
{
    public class CategoryController : Controller
    {
        // DbContext (chỉ khai báo 1 lần)
        private BookStoreDBContext db = new BookStoreDBContext();

        // GET: Category
        public ActionResult Index()
        {
            var categories = db.Categories
                               .Include(c => c.Books)
                               .ToList();

            return View(categories);
        }

        // GET: Category/Details/5
        public ActionResult Details(int id)
        {
            var category = db.Categories
                .Include(c => c.Books.Select(b => b.Author))
                .FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return View(category);
        }
    }
}
