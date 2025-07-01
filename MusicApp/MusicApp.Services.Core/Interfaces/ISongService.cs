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
    }
}
