using System.Diagnostics;
using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Web.Models;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly MusicAppDbContext context;
        public HomeController(MusicAppDbContext context)
        {
           this.context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var songs = context.Songs
             .Where(s => !s.IsDeleted)
             .OrderByDescending(s => s.Likes)
             .ThenBy(s=>s.Title)
             .Take(10)
             .Select(s => new SongViewModel
             {
                 Id = s.Id,
                 Title = s.Title,
                 Artist = s.Artist,
                 ImageUrl = s.ImageUrl ?? "/images/default-song.png",
                 AudioUrl = s.AudioUrl
             })
             .ToList();

            return View(songs);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode)
        {
            switch (statusCode)
            {
                case 401:
                case 403:
                    return this.View("UnauthorizedError");
                case 404:
                    return this.View("NotFoundError");
                case 500:
                    return this.View("BadRequestError");
                default:
                    return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
