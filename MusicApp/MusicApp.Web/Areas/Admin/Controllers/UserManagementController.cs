using Microsoft.AspNetCore.Mvc;

namespace MusicApp.Web.Areas.Admin.Controllers
{
    public class UserManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
