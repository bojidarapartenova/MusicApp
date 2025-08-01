using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Services.Core.Interfaces
{
    public interface IUserService
    {
        Task<int> GetUsersCountAsync();
    }
}
