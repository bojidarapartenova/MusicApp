using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace MusicApp.Web.Controllers
{
    public class PlaylistsController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
