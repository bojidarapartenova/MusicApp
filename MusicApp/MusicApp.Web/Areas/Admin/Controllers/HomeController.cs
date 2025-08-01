using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Services.Core;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Web.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        private readonly IUserService userService;
        private readonly ISongService songService;
        private readonly ICommentService commentService;

        public HomeController(IUserService userService, ISongService songService, ICommentService commentService)
        {
            this.userService = userService;
            this.songService = songService;
            this.commentService = commentService;
        }

        public async Task<IActionResult> Index()
        {
            int totalUsers=await userService.GetUsersCountAsync();
            int totalSongs=await songService.GetSongsCountAsync();
            int totalComments=await commentService.GetCommentsCountAsync();
            int totalLikes = await songService.GetTotalSongsLikesAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalSongs = totalSongs;  
            ViewBag.TotalComments = totalComments;
            ViewBag.TotalLikes=totalLikes;

            IEnumerable<SongViewModel> lates = await songService.GetLatestSongsAsync();

            return View(lates);
        }
    }
}
