using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Song;

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
                    ImageUrl = s.ImageUrl,
                    //ReleaseDate = s.ReleaseDate
                })
                .ToArrayAsync();

            return songs;
        }
    }
}
