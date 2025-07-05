using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.Controllers
{
    public class MyReleasesController : BaseController
    {
        private readonly IMyReleasesService myReleasesService;

        public MyReleasesController(IMyReleasesService myReleasesService)
        {
            this.myReleasesService = myReleasesService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<SongViewModel>? songs=await myReleasesService
                .GetAllMyReleasesAsync(GetUserId()!);

            return View(songs);
        }
    }
}
