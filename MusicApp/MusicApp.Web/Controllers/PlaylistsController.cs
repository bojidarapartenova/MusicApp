using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Data.Models;
using MusicApp.Services.Core;
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
        public async Task<IActionResult> Index(int page=1)
        {
            const int pageSize = 9;

            (IEnumerable<Playlist> playlists, int totalCount) = await playlistsService
                .GetUserPlaylistsAsync(GetUserId()!, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(playlists);
        }

        [HttpGet]
        public async Task<IActionResult> New()
        {
            try
            {
                var (songs, _) = await songService.GetAllSongsAsync();

                var viewModel = new CreatePlaylistViewModel
                {
                    Songs = songs.Select(s => new SongViewModel
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        ImageUrl = s.ImageUrl
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> New(CreatePlaylistViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                var userId = GetUserId()!;
                await playlistsService.CreatePlaylistAsync(viewModel, userId);
                return RedirectToAction(nameof(Index));
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

        [HttpGet]
        public async Task<IActionResult> ViewPlaylist(Guid id)
        {
            try
            {
                var playlist = await playlistsService.GetPlaylistDetailsAsync(id);

                if (playlist == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View(playlist);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSong(Guid playlistId, Guid songId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(nameof(Index));
                }

                bool result = await playlistsService.RemoveSongAsync(playlistId, songId);

                return RedirectToAction(nameof(ViewPlaylist), new { id = playlistId });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSongsToPlaylist(Guid playlistId, List<Guid> selectedSongIds)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(nameof(Index));
                }
                if (selectedSongIds != null && selectedSongIds.Any())
                {
                    await playlistsService.AddSongsToPlaylistAsync(playlistId, selectedSongIds);
                }

                return RedirectToAction(nameof(ViewPlaylist), new { id = playlistId });
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
                EditPlaylistInputModel? playlistToEdit = await
                    playlistsService.GetPlaylistToEditAsync(userId, id);

                if (playlistToEdit == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View(playlistToEdit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditPlaylistInputModel inputModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(inputModel);
                }
                bool result = await playlistsService.EditPlaylistAsync(inputModel);

                if (result == false)
                {
                    return View(inputModel);
                }
                return RedirectToAction(nameof(ViewPlaylist), new { id = inputModel.Id });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
