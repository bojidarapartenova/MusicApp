using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Playlists;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.Controllers
{
    public class PlaylistsController : BaseController
    {
        private readonly IPlaylistsService playlistsService;
        private readonly ISongService songService;
        public PlaylistsController(IPlaylistsService playlistsService, ISongService songService)
        {
            this.playlistsService = playlistsService;
            this.songService = songService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId()!;
            await playlistsService.EnsureFavoritesPlaylistExistsAsync(userId);

            var playlists = await playlistsService
                .GetUserPlaylistsAsync(GetUserId()!);

            return View(playlists);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            var songs = await songService.GetAllSongsAsync();

            var viewModel = new CreatePlaylistViewModel
            {
                Songs = songs.Select(s => new SongViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Artist = s.Artist
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> New(CreatePlaylistViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var userId=GetUserId()!;

            await playlistsService.CreatePlaylistAsync(viewModel, userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string? id)
        {
            try
            {
                string userId = GetUserId()!;
                DeletePlaylistViewModel? playlistToDelete = await playlistsService
                    .GetPlaylistToDeleteAsync(userId, id);

                if (playlistToDelete == null)
                {
                    return RedirectToAction(nameof(Index));
                }
                return View(playlistToDelete);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(DeletePlaylistViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }
                bool result = await playlistsService.SoftDeletePlaylistAsync(GetUserId()!, viewModel);

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

    }
}
