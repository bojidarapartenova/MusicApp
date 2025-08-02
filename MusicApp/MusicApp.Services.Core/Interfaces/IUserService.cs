using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Admin.UserManagement;

namespace MusicApp.Services.Core.Interfaces
{
    public interface IUserService
    {
        Task<int> GetUsersCountAsync();
        Task<IEnumerable<UserManagementIndexViewModel>> GetAllUsersAsync(string? searchTerm, string? roleFilter);
        Task MakeAdminAsync(string userId);
        Task RemoveAdminAsync(string userId);
        Task ToggleActivationAsync(string userId);
    }
}
