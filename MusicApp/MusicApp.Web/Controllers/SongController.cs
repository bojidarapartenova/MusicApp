using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.Controllers
{
    public class SongController : BaseController
    {
        private readonly ISongService songService;

        public SongController(ISongService songService)
        {
            this.songService = songService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IEnumerable<SongViewModel> songs =await songService.GetAllSongsAsync();

            return View(songs);
        }
    }
}
