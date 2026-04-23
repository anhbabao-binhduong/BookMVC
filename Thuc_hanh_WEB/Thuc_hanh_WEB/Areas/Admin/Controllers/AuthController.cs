using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Thuc_hanh_WEB.Models;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        private BookStoreDBContext db = new BookStoreDBContext();

        // GET: Admin/Auth/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Admin/Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập email và mật khẩu.";
                return View();
            }

            // 🔥 B1: tìm user theo email + role
            var user = db.Users
                .FirstOrDefault(u => u.Email.ToLower() == email.ToLower().Trim()
                                  && u.Role == "Admin");

            // 🔥 B2: kiểm tra mật khẩu bằng BCrypt
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                Session["AdminId"] = user.UserID;
                Session["AdminName"] = user.FullName;
                Session["AdminRole"] = user.Role;

                FormsAuthentication.SetAuthCookie(user.Email, false);

                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            ViewBag.Error = "Sai email/mật khẩu hoặc không có quyền Admin.";
            return View();
        }
        // Truy cập: /Admin/Auth/GenerateHash
        public ActionResult GenerateHash()
        {
            string hash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            return Content(hash);
        }
        // GET: Admin/Auth/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}