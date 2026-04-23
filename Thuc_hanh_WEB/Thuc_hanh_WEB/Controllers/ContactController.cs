using System.Web.Mvc;

namespace Thuc_hanh_WEB.Controllers
{
    public class ContactController : Controller
    {
        // GET: /Contact
        public ActionResult Index()
        {
            return View();
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string FullName, string Email, string Phone, string Subject, string Message)
        {
            if (ModelState.IsValid)
            {
                // TODO: Gửi email hoặc lưu vào DB
                // Ví dụ: SmtpClient / System.Net.Mail

                TempData["Success"] = "Cảm ơn " + FullName + "! Chúng tôi sẽ liên hệ lại với bạn sớm nhất.";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
