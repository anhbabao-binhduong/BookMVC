using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Thuc_hanh_WEB.Helpers;
using Thuc_hanh_WEB.Models;
using BCrypt.Net;
namespace Thuc_hanh_WEB.Controllers
{
    public class AccountController : Controller
    {
    private BookStoreDBContext db = new BookStoreDBContext();

    // ===============================================================
    // LOGIN
    // ===============================================================

    [HttpGet]
    public ActionResult Login(string returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public ActionResult Login(string email, string password, string returnUrl)
    {
        email = email?.Trim();
        var user = db.Users.FirstOrDefault(u => u.Email == email);

        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            Session["UserID"] = user.UserID;
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Email hoặc mật khẩu không đúng";
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // ===============================================================
    // LOGOUT
    // ===============================================================

    public ActionResult Logout()
    {
        Session.Remove("UserID");
        Session.Remove("FullName");
        Session.Remove("Role");
        // ❌ KHÔNG xóa CART
        return RedirectToAction("Login");
    }
    public string TestBcrypt(string email, string plainPassword)
    {
        var user = db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return "User not found";

        bool isValid = BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash);
        return $"Verify result: {isValid}. Hash in DB: {user.PasswordHash}";
    }
    // ===============================================================
    // REGISTER
    // ===============================================================

    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");
            return View(model);
        }

        if (!Regex.IsMatch(model.Phone, @"^(03|05|07|08|09)\d{8}$"))
        {
            ModelState.AddModelError("Phone", "Số điện thoại không hợp lệ");
            return View(model);
        }

        if (!Regex.IsMatch(model.Password,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$"))
        {
            ModelState.AddModelError("Password",
                "Mật khẩu phải ≥8 ký tự, gồm hoa, thường, số và ký tự đặc biệt");
            return View(model);
        }

        if (db.Users.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email đã được đăng ký");
            return View(model);
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

        string code = new Random().Next(1000, 9999).ToString();
        var verify = db.EmailVerifications.FirstOrDefault(v => v.Email == model.Email);

        if (verify == null)
        {
            db.EmailVerifications.Add(new EmailVerification
            {
                Email = model.Email,
                VerifyCode = code,
                FailedCount = 0,
                ExpiredAt = DateTime.Now.AddMinutes(5)
            });
        }
        else
        {
            verify.VerifyCode = code;
            verify.FailedCount = 0;
            verify.ExpiredAt = DateTime.Now.AddMinutes(5);
        }

        db.SaveChanges();
        EmailHelper.Send(model.Email, code);

        Session["RegisterEmail"] = model.Email;
        Session["RegisterName"] = model.FullName;
        Session["RegisterPhone"] = model.Phone;
        Session["RegisterPasswordHash"] = passwordHash;

        return RedirectToAction("VerifyCode");
    }

    // ===============================================================
    // VERIFY CODE
    // ===============================================================

    [HttpGet]
    public ActionResult VerifyCode() => View();

    [HttpPost]
    public ActionResult VerifyCode(string code)
    {
        string email = Session["RegisterEmail"] as string;
        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Register");

        var verify = db.EmailVerifications.FirstOrDefault(v => v.Email == email);

        if (verify == null || verify.ExpiredAt < DateTime.Now)
        {
            ViewBag.Error = "Mã xác thực đã hết hạn. Vui lòng đăng ký lại.";
            return View();
        }

        if (verify.VerifyCode != code)
        {
            verify.FailedCount++;
            db.SaveChanges();

            if (verify.FailedCount >= 3)
            {
                db.EmailVerifications.Remove(verify);
                db.SaveChanges();

                Session.Remove("RegisterEmail");
                Session.Remove("RegisterName");
                Session.Remove("RegisterPhone");
                Session.Remove("RegisterPasswordHash");

                TempData["RegisterFail"] =
                    "Bạn đã nhập sai mã xác thực quá 3 lần. Đăng ký không thành công.";
                return RedirectToAction("Register");
            }

            ViewBag.Error = $"Mã xác thực không đúng. Bạn còn {3 - verify.FailedCount} lần thử.";
            return View();
        }

        var user = new Users
        {
            FullName = Session["RegisterName"].ToString(),
            Email = email,
            Phone = Session["RegisterPhone"].ToString(),
            PasswordHash = Session["RegisterPasswordHash"].ToString(),
            Role = "Customer",
            CreatedAt = DateTime.Now
        };

        db.Users.Add(user);
        db.EmailVerifications.Remove(verify);
        db.SaveChanges();

        Session.Remove("RegisterEmail");
        Session.Remove("RegisterName");
        Session.Remove("RegisterPhone");
        Session.Remove("RegisterPasswordHash");

        TempData["RegisterSuccess"] = "Xác thực thành công. Bạn có thể đăng nhập.";
        return RedirectToAction("Login");
    }

    // ===============================================================
    // FORGOT PASSWORD
    // ===============================================================

    [HttpPost]
    public ActionResult ForgotPassword(string email)
    {
        email = email?.Trim().ToLower();
        var user = db.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
            return Json(new { success = false, message = "Email này chưa được đăng ký." });

        string token = Guid.NewGuid().ToString("N");
        Session["ResetToken_" + token] = email;
        Session["ResetToken_Expire_" + token] = DateTime.Now.AddMinutes(15);

        string resetUrl = Url.Action("ResetPassword", "Account",
            new { token }, Request.Url.Scheme);

        EmailHelper.Send(email, "Đặt lại mật khẩu BookStore", resetUrl);

        return Json(new { success = true });
    }

    // ===============================================================
    // RESET PASSWORD
    // ===============================================================

    [HttpGet]
    public ActionResult ResetPassword(string token)
    {
        string email = Session["ResetToken_" + token] as string;
        DateTime? expire = Session["ResetToken_Expire_" + token] as DateTime?;

        if (string.IsNullOrEmpty(email) || expire == null || expire < DateTime.Now)
        {
            ViewBag.InvalidToken = true;
            return View();
        }

        ViewBag.Token = token;
        return View();
    }

    [HttpPost]
    public ActionResult ResetPassword(string token, string newPassword, string confirmPassword)
    {
        string email = Session["ResetToken_" + token] as string;
        DateTime? expire = Session["ResetToken_Expire_" + token] as DateTime?;

        if (string.IsNullOrEmpty(email) || expire == null || expire < DateTime.Now)
        {
            ViewBag.InvalidToken = true;
            return View();
        }

        ViewBag.Token = token;

        if (newPassword != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu xác nhận không khớp.";
            return View();
        }

        if (!Regex.IsMatch(newPassword,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$"))
        {
            ViewBag.Error = "Mật khẩu phải >= 8 ký tự, gồm chữ hoa, chữ thường, ký tự đặc biệt, số.";
            return View();
        }

        var user = db.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            ViewBag.Error = "Không tìm thấy tài khoản.";
            return View();
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        db.SaveChanges();

        Session.Remove("ResetToken_" + token);
        Session.Remove("ResetToken_Expire_" + token);

        TempData["ResetSuccess"] = "Mật khẩu đã được cập nhật, vui lòng đăng nhập lại.";
        return RedirectToAction("Login");
    }

    // ===============================================================
    // PROFILE  — yêu cầu đăng nhập
    // ===============================================================

    [HttpGet]
    [ActionName("Profile")]
    public ActionResult MyProfile()
    {
        int? userId = Session["UserID"] as int?;
        if (userId == null) return RedirectToAction("Login");

        var user = db.Users.Find(userId.Value);
        if (user == null) return RedirectToAction("Login");

        return View("Profile", user);
    }

    // ---------------------------------------------------------------
    // Cập nhật thông tin cá nhân (FullName, Phone, Address)
    // ---------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UpdateProfile(Users model)
    {
        int? sessionId = Session["UserID"] as int?;
        if (sessionId == null || sessionId.Value != model.UserID)
            return RedirectToAction("Login");

        // Không validate PasswordHash trong form này
        ModelState.Remove("PasswordHash");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin.";
            return View("Profile", model);
        }

        // Validate phone nếu có nhập
        if (!string.IsNullOrWhiteSpace(model.Phone) &&
            !Regex.IsMatch(model.Phone, @"^(03|05|07|08|09)\d{8}$"))
        {
            TempData["Error"] = "Số điện thoại không hợp lệ (VD: 0912345678).";
            return View("Profile", model);
        }

        var user = db.Users.Find(model.UserID);
        if (user == null) return HttpNotFound();

        user.FullName = model.FullName?.Trim();
        user.Phone = model.Phone?.Trim();
        user.Address = model.Address?.Trim();

        db.SaveChanges();

        // Cập nhật lại Session tên hiển thị
        Session["FullName"] = user.FullName;

        TempData["Success"] = "Cập nhật thông tin thành công!";
        return RedirectToAction("Profile");
    }

    // ---------------------------------------------------------------
    // Đổi mật khẩu
    // ---------------------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ChangePassword(int UserID, string CurrentPassword,
                                       string NewPassword, string ConfirmPassword)
    {
        int? sessionId = Session["UserID"] as int?;
        if (sessionId == null || sessionId.Value != UserID)
            return RedirectToAction("Login");

        var user = db.Users.Find(UserID);
        if (user == null) return HttpNotFound();

        // Kiểm tra mật khẩu hiện tại (BCrypt)
        if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
        {
            TempData["Error"] = "Mật khẩu hiện tại không đúng.";
            return RedirectToAction("Profile");
        }

        if (string.IsNullOrWhiteSpace(NewPassword) ||
            !Regex.IsMatch(NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$"))
        {
            TempData["Error"] = "Mật khẩu mới phải ≥8 ký tự, gồm hoa, thường, số và ký tự đặc biệt.";
            return RedirectToAction("Profile");
        }

        if (NewPassword != ConfirmPassword)
        {
            TempData["Error"] = "Xác nhận mật khẩu không khớp.";
            return RedirectToAction("Profile");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
        db.SaveChanges();

        TempData["Success"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("Profile");
    }

    // ---------------------------------------------------------------
    // Xóa tài khoản
    // ---------------------------------------------------------------
    [HttpGet]
    public ActionResult DeleteAccount()
    {
        int? userId = Session["UserID"] as int?;
        if (userId == null) return RedirectToAction("Login");

        var user = db.Users.Find(userId.Value);
        if (user != null)
        {
            db.Users.Remove(user);
            db.SaveChanges();
        }

        Session.Remove("UserID");
        Session.Remove("FullName");
        Session.Remove("Role");

        return RedirectToAction("Login");
    }

    // ===============================================================
    // CREATE NEW ADMIN (tạm thời, chỉ dùng để khắc phục lỗi đăng nhập)
    // ===============================================================
    public ActionResult CreateNewAdmin()
    {
        string adminEmail = "admin@bookstore.com";
        var existingAdmin = db.Users.FirstOrDefault(u => u.Email == adminEmail);
        if (existingAdmin != null)
        {
            // Nếu đã tồn tại, xóa cũ rồi tạo mới
            db.Users.Remove(existingAdmin);
            db.SaveChanges();
        }

        string plainPassword = "123456";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        Users newAdmin = new Users
        {
            FullName = "Administrator",
            Email = adminEmail,
            PasswordHash = passwordHash,
            Phone = "0987654321",
            Address = "Admin Office",
            Role = "Admin",
            CreatedAt = DateTime.Now
        };

        db.Users.Add(newAdmin);
        db.SaveChanges();

        return Content($"<h3>✅ Tạo admin thành công!</h3>" +
                       $"<p>Email: <strong>{adminEmail}</strong></p>" +
                       $"<p>Mật khẩu: <strong>{plainPassword}</strong></p>" +
                       $"<p>Hash: <code>{passwordHash}</code></p>" +
                       $"<p>Độ dài hash: {passwordHash.Length} ký tự (phải là 60).</p>" +
                       $"<p>👉 <a href='/Admin/Auth/Login'>Đăng nhập Admin ngay</a></p>");
    }
}
}