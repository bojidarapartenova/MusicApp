using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Data.Models;
using MusicApp.Web.ViewModels.Playlists;

namespace MusicApp.Services.Core.Interfaces
{
    public interface IPlaylistsService
    {
        Task<ICollection<Playlist>> GetUserPlaylistsAsync(string userId);
        Task CreatePlaylistAsync(CreatePlaylistViewModel viewModel, string userId);
    }
}
