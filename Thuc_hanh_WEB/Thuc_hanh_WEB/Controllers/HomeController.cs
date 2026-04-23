using System.Linq;
using System.Web.Mvc;
using Thuc_hanh_WEB.Models;
using System.Data.Entity;
namespace Thuc_hanh_WEB.Controllers
{
    public class HomeController : Controller
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        public ActionResult Index()
        {
            var featuredBooks = db.Books
                                  .Include(b => b.Author)
                                  .OrderByDescending(b => b.CreatedAt)
                                  .Take(8)
                                  .ToList();

            return View(featuredBooks);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult Hash()
        {
            return Content(BCrypt.Net.BCrypt.HashPassword("123456"));
        }
    }

}
