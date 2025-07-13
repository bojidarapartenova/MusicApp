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
        Task<ICollection<Playlist>> GetUserPlaylistsAsync(string userId);
        Task CreatePlaylistAsync(CreatePlaylistViewModel viewModel, string userId);
        Task<DeletePlaylistViewModel?> GetPlaylistToDeleteAsync(string userId, string? playlistId);
        Task<bool> SoftDeletePlaylistAsync(string userId, DeletePlaylistViewModel viewModel);
    }
}
