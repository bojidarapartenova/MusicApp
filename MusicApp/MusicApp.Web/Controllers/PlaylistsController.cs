using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Playlists;

namespace MusicApp.Web.Controllers
{
    public class PlaylistsController : BaseController
    {
        private readonly IPlaylistsService playlistsService;

        public PlaylistsController(IPlaylistsService playlistsService)
        {
            this.playlistsService = playlistsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var playlists = await playlistsService
                .GetUserPlaylistsAsync(GetUserId()!);

            return View(playlists);
        }
    }
}
