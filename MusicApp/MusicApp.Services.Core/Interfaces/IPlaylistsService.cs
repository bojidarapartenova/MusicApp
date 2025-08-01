using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Data.Models;
using MusicApp.Web.ViewModels.Playlists;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Services.Core.Interfaces
{
    public interface IPlaylistsService
    {
        Task EnsureFavoritesPlaylistExistsAsync(string userId);
        Task<Playlist?> GetFavoritesPlaylistAsync(string userId);
        Task<(IEnumerable<Playlist> Playlists, int totalCount)> GetUserPlaylistsAsync(string userId, int? page=null, int? pageSize=null);
        Task CreatePlaylistAsync(CreatePlaylistViewModel viewModel, string userId);
        Task<DeletePlaylistViewModel?> GetPlaylistToDeleteAsync(string userId, string? playlistId);
        Task<bool> SoftDeletePlaylistAsync(string userId, DeletePlaylistViewModel viewModel);
        Task<IEnumerable<SongViewModel>> GetAllSongsInPlaylistAssync(string playlistId);
        Task<PlaylistDetailsViewModel?> GetPlaylistDetailsAsync(Guid playlistId);
        Task<bool> RemoveSongAsync(Guid playlistId, Guid songId);
        Task AddSongToFavoritesAsync(string userId, Guid songId);
        Task RemoveSongFromFavoritesAsync(string userId, Guid songId);
        Task<bool> IsSongFavoritesAsync(string userId, Guid songId);
        Task AddSongsToPlaylistAsync(Guid playlistId, List<Guid> songIds);
        Task<EditPlaylistInputModel?> GetPlaylistToEditAsync(string userId, string? playlistId);
        Task<bool> EditPlaylistAsync(EditPlaylistInputModel inputModel);
    }
}
