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
    public class GenreService : IGenreService
    {
        private readonly MusicAppDbContext dbContext;

        public GenreService(MusicAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IEnumerable<AddSongGenreDropDown>> GetGenresDropDownAsync()
        {
            IEnumerable<AddSongGenreDropDown> genres = await dbContext
                .Genres
                .AsNoTracking()
                .Select(g => new AddSongGenreDropDown()
                {
                    GenreId = g.Id,
                    Name = g.Name
                })
                .ToArrayAsync();

            return genres;
        }
    }
}
