using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Thuc_hanh_WEB.Areas.Admin
{
    public class AuthorController : Controller
    {
        // GET: Admin/Author
        public ActionResult Index()
        {
            return View();
        }
    }
}