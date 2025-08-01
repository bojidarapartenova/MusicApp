using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Interfaces;

namespace MusicApp.Services.Core
{
    public class UserService : IUserService
    {
        private readonly MusicAppDbContext dbContext;

        public UserService(MusicAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<int> GetUsersCountAsync()
        {
            int users = await dbContext
                .Users
                .CountAsync();

            return users;
        }
    }
}
