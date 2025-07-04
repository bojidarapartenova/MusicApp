using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Services.Core.Interfaces
{
    public interface ISongService
    {
        Task<IEnumerable<SongViewModel>> GetAllSongsAsync();
        Task <bool> AddSongAsync(string userId, AddSongInputModel inputModel);
        Task<EditSongInputModel?> GetSongToEditAsync(string userId, string? songId);
        Task<bool> EditSongAsync(EditSongInputModel inputModel);
    }
}
