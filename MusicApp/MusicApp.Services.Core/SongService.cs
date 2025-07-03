using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;
using MusicApp.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace MusicApp.Services.Core
{
    public class SongService : ISongService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;
        public SongService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<IEnumerable<SongViewModel>> GetAllSongsAsync()
        {
            IEnumerable<SongViewModel> songs = await dbContext
                .Songs
                .AsNoTracking()
                .Select(s => new SongViewModel()
                {
                    Id = s.Id,
                    Title = s.Title,
                    Duration = s.Duration,
                    ImageUrl = s.ImageUrl ?? $"/images/no-image.jpg"
                })
                .ToArrayAsync();

            return songs;
        }

        public async Task<bool> AddSongAsync(string userId, AddSongInputModel inputModel)
        {
            bool result = false;

            ApplicationUser? user=await userManager.FindByIdAsync(userId);
            Genre? genre = await dbContext.Genres.FindAsync(inputModel.GenreId);

            if (user != null && genre!=null)
            {
                Song song = new Song()
                {
                    Title = inputModel.Title,
                    PublisherId = userId,
                    GenreId = inputModel.GenreId,
                    Artist = inputModel.Artist,
                    Duration = inputModel.Duration,
                    ReleaseDate = DateTime.Now,
                    Likes = 0,
                    ImageUrl = inputModel.ImageUrl?? $"/images/no-image.jpg",
                    AudioUrl = inputModel.AudioUrl
                };

                await dbContext.Songs.AddAsync(song);
                await dbContext.SaveChangesAsync();

                result = true;
            }
            return result;
        }
    }
}
