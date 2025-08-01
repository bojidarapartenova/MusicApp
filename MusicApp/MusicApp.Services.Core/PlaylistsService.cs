using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Playlists;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Services.Core
{
    public class PlaylistsService : IPlaylistsService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public PlaylistsService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        public async Task<(IEnumerable<Playlist> Playlists, int totalCount)> GetUserPlaylistsAsync(string userId, int? page = null, int? pageSize = null)
        {
            IEnumerable<Playlist> playlists = await dbContext
                .Playlists
                .Where(p => p.UserId.ToLower() == userId.ToLower())
                .ToListAsync();

            int totalCount = playlists.Count();

            if(page.HasValue && pageSize.HasValue)
            {
                playlists=playlists
                    .Skip((page.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value);
            }


            return (playlists, totalCount);
        }

        public async Task CreatePlaylistAsync(CreatePlaylistViewModel viewModel, string userId)
        {
            Playlist playlist = new Playlist()
            {
                Id = Guid.NewGuid(),
                Title = viewModel.Title,
                ImageUrl = viewModel.ImageUrl ?? $"/images/no-image.jpg",
                UserId = userId
            };

            foreach (var songId in viewModel.SelectedSongsIds)
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
            bool exists = await dbContext
                .Playlists
                .AnyAsync(p => p.UserId.ToLower() == userId.ToLower() && p.IsDefault);

            if (!exists)
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
                .Include(p => p.PlaylistsSongs)
                .FirstOrDefaultAsync(p => p.UserId.ToLower() == userId && p.IsDefault);
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
                    ImageUrl = ps.Song.ImageUrl ?? $"/images/no-image.jpg",
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
            PlaylistDetailsViewModel? viewModel = null;

            var playlist = await dbContext
                .Playlists
                .Include(p => p.PlaylistsSongs)
                    .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == playlistId && !p.IsDeleted);

            if (playlist != null)
            {
                var songsInPlaylist = playlist.PlaylistsSongs
                    .Select(ps => ps.Song)
                    .ToList();

                var allSongs = await dbContext.Songs
                    .Where(s => !s.IsDeleted)
                    .ToListAsync();

                var availableSongs = allSongs
                    .Where(s => songsInPlaylist.All(pSong => pSong.Id != s.Id))
                    .ToList();


                viewModel= new PlaylistDetailsViewModel
                {
                    Id = playlist.Id,
                    Title = playlist.Title,
                    IsDeafault = playlist.IsDefault,

                    Songs = songsInPlaylist.Select(s => new SongViewModel
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        ImageUrl = s.ImageUrl ?? $"/images/no-image.jpg",
                        AudioUrl = s.AudioUrl
                    }).ToList(),

                    AvailableSongs = availableSongs.Select(s => new SongViewModel
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Artist = s.Artist,
                        ImageUrl = s.ImageUrl ?? $"/images/no-image.jpg"
                    }).ToList()
                };
            }
            return viewModel;
        }


        public async Task<bool> RemoveSongAsync(Guid playlistId, Guid songId)
        {
            bool result = false;

            var songToRemove = await dbContext
                .PlaylistsSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

            if (songToRemove != null)
            {
                dbContext.PlaylistsSongs.Remove(songToRemove);
                await dbContext.SaveChangesAsync();
                result = true;
            }

            return result;
        }

        public async Task AddSongToFavoritesAsync(string userId, Guid songId)
        {
            await EnsureFavoritesPlaylistExistsAsync(userId); 

            var favorites = await dbContext
                .Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstOrDefaultAsync(p => p.UserId.ToLower() == userId.ToLower() && p.IsDefault);

            bool exists = favorites!.PlaylistsSongs.Any(ps => ps.SongId == songId);

            if (!exists)
            {
                favorites.PlaylistsSongs.Add(new PlaylistSong()
                {
                    PlaylistId = favorites.Id,
                    SongId = songId
                });
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveSongFromFavoritesAsync(string userId, Guid songId)
        {
            var favorites = await dbContext
               .Playlists
               .Include(p => p.PlaylistsSongs)
               .FirstOrDefaultAsync(p => p.UserId.ToLower() == userId.ToLower() && p.IsDefault);

            var entry = favorites!.PlaylistsSongs.FirstOrDefault(ps => ps.SongId == songId);

            if (entry != null)
            {
                dbContext.Remove(entry);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> IsSongFavoritesAsync(string userId, Guid songId)
        {
            var favorites = await dbContext
              .Playlists
              .Include(p => p.PlaylistsSongs)
              .FirstOrDefaultAsync(p => p.UserId.ToLower() == userId.ToLower() && p.IsDefault);

            return favorites!.PlaylistsSongs.Any(ps => ps.SongId == songId);
        }

        public async Task AddSongsToPlaylistAsync(Guid playlistId, List<Guid> songIds)
        {
            var playlist = await dbContext
                .Playlists
                .Include(p => p.PlaylistsSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist != null)
            {
                foreach (var songId in songIds)
                {
                    if (!playlist.PlaylistsSongs.Any(ps => ps.SongId == songId))
                    {
                        playlist.PlaylistsSongs.Add(new PlaylistSong()
                        {
                            PlaylistId = playlistId,
                            SongId = songId
                        });
                    }
                }
            }
            await dbContext.SaveChangesAsync();
        }

        public async Task<EditPlaylistInputModel?> GetPlaylistToEditAsync(string userId, string? playlistId)
        {
            ApplicationUser? user=await userManager.FindByIdAsync(userId);
            EditPlaylistInputModel? playlistToEdit= null;

            bool isGuidValid=Guid.TryParse(playlistId, out Guid id);

            if(isGuidValid)
            {
                Playlist? playlist = await dbContext.Playlists.FindAsync(id);

                if(playlist != null)
                {
                    playlistToEdit = new EditPlaylistInputModel()
                    {
                        Title = playlist.Title,
                        ImageUrl = playlist.ImageUrl ?? $"/images/no-image.jpg"
                    };
                }
            }
            return playlistToEdit;
        }

        public async Task<bool> EditPlaylistAsync(EditPlaylistInputModel inputModel)
        {
            bool result = false;

            Playlist? editedPlaylist = await dbContext.Playlists.FindAsync(inputModel.Id);

            if(editedPlaylist != null)
            {
                editedPlaylist.Title = inputModel.Title;
                editedPlaylist.ImageUrl = inputModel.ImageUrl;

                await dbContext.SaveChangesAsync();
                result = true;
            }
            return result;
        }
    }
}
