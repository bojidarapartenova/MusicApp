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

namespace MusicApp.Services.Core
{
    public class SongService : ISongService
    {
        private readonly MusicAppDbContext dbContext;

        public SongService(MusicAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IEnumerable<SongViewModel>> GetAllSongsAsync()
        {
            IEnumerable<SongViewModel> songs = await dbContext
                .Songs
                .AsNoTracking()
                .Select(s => new SongViewModel()
                {
                    Id=s.Id,
                    Title = s.Title,
                    Duration = s.Duration,
                    ImageUrl = s.ImageUrl ?? $"/images/no-image.jpg"
                })
                .ToArrayAsync();

            return songs;
        }

        public async Task AddSongAsync(AddSongInputModel inputModel)
        {
            Song song = new Song()
            {
                Title = inputModel.Title,
                ImageUrl = inputModel.ImageUrl,
                AudioUrl = inputModel.AudioUrl,
                ReleaseDate = DateTime.Now,
                Duration = inputModel.Duration,
                Likes = 0
            };

            await dbContext.Songs.AddAsync(song);
            await dbContext.SaveChangesAsync();
        }
    }
}
