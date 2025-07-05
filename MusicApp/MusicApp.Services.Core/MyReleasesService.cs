using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Services.Core
{
    public class MyReleasesService : IMyReleasesService
    {
        private readonly MusicAppDbContext dbContext;
        private UserManager<ApplicationUser> userManager;

        public MyReleasesService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        public async Task<IEnumerable<SongViewModel>> GetAllMyReleasesAsync(string userId)
        {
            IEnumerable<SongViewModel> myReleases = await dbContext
                .Songs
                .Where(s => s.PublisherId.ToLower() == userId.ToLower())
                .Select(s => new SongViewModel
                {
                    Id = s.Id,
                    ImageUrl = s.ImageUrl,
                    Title = s.Title,
                    Duration = s.Duration,
                    Artist = s.Artist,
                    PublisherId = s.PublisherId,
                    Publisher = s.Publisher.UserName ?? s.Publisher.Email!,
                    Genre = s.Genre.Name,
                    ReleaseDate = s.ReleaseDate
                })
                .ToArrayAsync();

            return myReleases;
        }
    }
}
