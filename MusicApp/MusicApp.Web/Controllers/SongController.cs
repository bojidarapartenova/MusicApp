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
        private readonly IGenreService genreService;
        private readonly IPlaylistsService playlistService;
        public SongController(ISongService songService, IGenreService genreService, IPlaylistsService playlistsService)
        {
            this.songService = songService;
            this.genreService = genreService;
            this.playlistService = playlistsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IEnumerable<SongViewModel> songs = await songService.GetAllSongsAsync();

            return View(songs);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            try
            {
                AddSongInputModel inputModel = new AddSongInputModel
                {
                    Genres = await genreService.GetGenresDropDownAsync()
                };

                return View(inputModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddSongInputModel inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(inputModel);
                }
                bool result = await songService.AddSongAsync(GetUserId()!, inputModel);

                if (!result)
                {
                    return View(inputModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id)
        {
            try
            {
                string userId = GetUserId()!;
                EditSongInputModel? songToEdit = await
                    songService.GetSongToEditAsync(userId, id);

                if (songToEdit == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                songToEdit.Genres = await genreService.GetGenresDropDownAsync();

                return View(songToEdit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSongInputModel inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        return View(inputModel);
                    }
                }
                bool result = await songService.EditSongAsync(inputModel);

                if (result == false)
                {
                    return View(inputModel);
                }
                return RedirectToAction(nameof(Index), new { showModal = true, id = inputModel.Id });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            try
            {
                string userId = GetUserId()!;
                DeleteSongViewModel? songToDelete = await
                    songService.GetSongToDeleteAsync(userId, id);

                if (songToDelete == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return View(songToDelete);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeleteSongViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                bool result = await songService.SoftDeleteSongAsync(GetUserId()!, viewModel);

                if (result == false)
                {
                    return View(viewModel);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Listen(string id)
        {
            try
            {
                SongViewModel? song = await songService.GetSongByIdAsync(id);

                if (song == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View(song);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
