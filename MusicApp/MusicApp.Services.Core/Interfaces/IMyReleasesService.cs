using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Song;

namespace MusicApp.Services.Core.Interfaces
{
    public interface IMyReleasesService
    {
        Task<IEnumerable<SongViewModel>> GetAllMyReleasesAsync(string userId);
    }
}
