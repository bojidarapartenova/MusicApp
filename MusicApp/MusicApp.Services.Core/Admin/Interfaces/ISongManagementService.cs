using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Admin.SongManagement;

namespace MusicApp.Services.Core.Admin.Interfaces
{
    public interface ISongManagementService
    {
        Task<IEnumerable<SongManagementViewModel>> GetAllSongsAsync(string? searchTerm);
        Task SoftDeleteAsync(Guid id);
        Task RestoreAsync(Guid id);

    }
}
