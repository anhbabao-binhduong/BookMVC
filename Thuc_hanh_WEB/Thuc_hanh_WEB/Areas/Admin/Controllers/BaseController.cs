using System.Web.Mvc;
using Thuc_hanh_WEB.Filters;

namespace Thuc_hanh_WEB.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public abstract class BaseController : Controller
    {
    }
}