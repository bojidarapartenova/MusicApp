using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;
using MusicApp.Services.Core;

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

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddSongInputModel inputModel)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return View(inputModel);
                }

                await songService.AddSongAsync(inputModel);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(inputModel);
            }
        }
    }
}
