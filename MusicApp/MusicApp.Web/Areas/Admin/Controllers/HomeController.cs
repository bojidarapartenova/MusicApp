using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MusicApp.Web.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
