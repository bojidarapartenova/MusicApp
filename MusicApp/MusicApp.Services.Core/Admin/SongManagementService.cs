using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Admin.Interfaces;
using MusicApp.Web.ViewModels.Admin.SongManagement;
using MusicApp.Data.Models;

namespace MusicApp.Services.Core.Admin
{
    public class SongManagementService:ISongManagementService
    {
        private readonly MusicAppDbContext dbContext;

        public SongManagementService(MusicAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IEnumerable<SongManagementViewModel>> GetAllSongsAsync(string? searchTerm)
        {
            IEnumerable<SongManagementViewModel> songs = await dbContext
                .Songs
                .AsNoTracking()
                .IgnoreQueryFilters()
                .OrderByDescending(s => s.ReleaseDate)
                .Select(s => new SongManagementViewModel()
                {
                    Id = s.Id,
                    Title = s.Title,
                    Genre = s.Genre.Name,
                    Publisher = s.Publisher.UserName,
                    ReleaseDate = s.ReleaseDate.ToString("yyyy-MM-dd"),
                    IsDeleted = s.IsDeleted
                })
                .ToListAsync();

            if (searchTerm != null)
            {
                songs = songs.Where(s => s.Title.ToLower().Contains(searchTerm.ToLower()));
            }

            return songs;
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            Song? song = await dbContext
                .Songs
                .FirstAsync(s => s.Id == id);

            if(song != null)
            {
                song.IsDeleted = true;
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task RestoreAsync(Guid id)
        {
            Song? song=await dbContext
                .Songs
                .IgnoreQueryFilters()
                .FirstAsync (s => s.Id == id);

            if(song != null)
            {
                song.IsDeleted = false;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
