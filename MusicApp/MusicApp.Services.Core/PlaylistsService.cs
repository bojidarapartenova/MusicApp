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
using MusicApp.Web.ViewModels.Song;

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

        public async Task CreatePlaylistAsync(CreatePlaylistViewModel viewModel, string userId)
        {
            var playlist = new Playlist
            {
                Id = Guid.NewGuid(),
                Title = viewModel.Title,
                ImageUrl = viewModel.ImageUrl,
                UserId = userId,
                PlaylistsSongs = viewModel.SelectedSongsIds.Select(songId => new PlaylistSong
                {
                    SongId = songId
                }).ToList()
            };

            dbContext.Playlists.Add(playlist);
            await dbContext.SaveChangesAsync();
        }

        public async Task<DeletePlaylistViewModel?> GetPlaylistToDeleteAsync(string userId, string? playlistId)
        {
            DeletePlaylistViewModel? playlistToDelete = null;

            bool isGuidValid = Guid.TryParse(playlistId, out Guid id);

            if (playlistId != null)
            {
                Playlist? playlist = await dbContext
                    .Playlists
                    .AsNoTracking()
                    .SingleOrDefaultAsync(s => s.Id == id);

                if (playlist != null)
                {
                    playlistToDelete = new DeletePlaylistViewModel()
                    {
                        Id = playlist.Id,
                        Title = playlist.Title
                    };
                }
            }
            return playlistToDelete;
        }

        public async Task<bool> SoftDeletePlaylistAsync(string userId, DeletePlaylistViewModel viewModel)
        {
            bool result = false;

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            Playlist? deletedPlaylist = await dbContext.Playlists.FindAsync(viewModel.Id);

            if (user != null && deletedPlaylist != null)
            {
                deletedPlaylist.IsDeleted = true;

                await dbContext.SaveChangesAsync();
                result = true;
            }
            return result;
        }
    }
}
