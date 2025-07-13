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
            Playlist playlist = new Playlist()
            {
                Id = Guid.NewGuid(),
                Title = viewModel.Title,
                ImageUrl = viewModel.ImageUrl,
                UserId = userId
            };

            foreach(var songId in viewModel.SelectedSongsIds)
            {
                playlist.PlaylistsSongs.Add(new PlaylistSong()
                {
                    PlaylistId = playlist.Id,
                    SongId = songId
                });
            }

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

        public async Task EnsureFavoritesPlaylistExistsAsync(string userId)
        {
            bool exists= await dbContext
                .Playlists
                .AnyAsync(p=>p.UserId.ToLower()==userId.ToLower() && p.IsDefault);

            if(!exists)
            {
                Playlist favorites = new Playlist
                {
                    Title = "Favorites",
                    IsDefault = true,
                    UserId = userId,
                    ImageUrl = "https://misc.scdn.co/liked-songs/liked-songs-300.jpg"
                };

                dbContext.Playlists.Add(favorites);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<Playlist?> GetFavoritesPlaylistAsync(string userId)
        {
            return await dbContext
                .Playlists
                .Include(p=>p.PlaylistsSongs)
                .FirstOrDefaultAsync(p=>p.UserId.ToLower()==userId && p.IsDefault);
        }

        public async Task<IEnumerable<SongViewModel>> GetAllSongsInPlaylistAssync(string playlistId)
        {
            if (!Guid.TryParse(playlistId, out var playlistGuid))
                return Enumerable.Empty<SongViewModel>(); // Invalid ID

            var songs = await dbContext
                .PlaylistsSongs
                .Where(ps => ps.PlaylistId == playlistGuid)
                .Include(ps => ps.Song)
                    .ThenInclude(s => s.Publisher)
                .Include(ps => ps.Song)
                    .ThenInclude(s => s.Genre)
                .Select(ps => new SongViewModel
                {
                    Id = ps.Song.Id,
                    ImageUrl = ps.Song.ImageUrl,
                    Title = ps.Song.Title,
                    Duration = ps.Song.Duration,
                    Artist = ps.Song.Artist,
                    PublisherId = ps.Song.PublisherId,
                    Publisher = ps.Song.Publisher.UserName ?? ps.Song.Publisher.Email!,
                    Genre = ps.Song.Genre.Name,
                    ReleaseDate = ps.Song.ReleaseDate
                })
                .ToArrayAsync();

            return songs;
        }

        public async Task<PlaylistDetailsViewModel?> GetPlaylistDetailsAsync(Guid playlistId)
        {
            var playlist = await dbContext
                .Playlists
                .Where(p => p.Id == playlistId)
                .Select(p => new PlaylistDetailsViewModel()
                {
                    Id = p.Id,
                    Title = p.Title,
                    Songs = p.PlaylistsSongs
                    .Select(ps => new SongViewModel()
                    {
                        Id = ps.SongId,
                        Title = ps.Song.Title,
                        Artist = ps.Song.Artist,
                        ImageUrl = ps.Song.ImageUrl,
                        AudioUrl = ps.Song.AudioUrl
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            return playlist;
        }

        public async Task<bool> RemoveSongAsync(Guid playlistId, Guid songId)
        {
            bool result = false;

            var songToRemove = await dbContext
                .PlaylistsSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if(songToRemove != null)
            {
                dbContext.PlaylistsSongs.Remove(songToRemove);
                await dbContext.SaveChangesAsync();
                result = true;
            }

            return result;
        }
    }
}
