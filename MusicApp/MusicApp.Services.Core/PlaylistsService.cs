using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Playlists;

namespace MusicApp.Services.Core
{
    public class PlaylistsService:IPlaylistsService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public PlaylistsService (MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<ICollection<Playlist>> GetUserPlaylistsAsync(string userId)
        {
            var playlists = await dbContext
                .Playlists
                .Where(p => p.UserId.ToLower() == userId.ToLower())
                .ToArrayAsync();

            return playlists;
        }
    }
}
