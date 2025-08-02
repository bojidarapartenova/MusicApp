using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Admin.Interfaces;
using MusicApp.Web.ViewModels.Admin.UserManagement;

namespace MusicApp.Services.Core.Admin
{
    public class UserService : IUserService
    {
        private readonly MusicAppDbContext dbContext;
        private readonly UserManager<ApplicationUser> userManager;

        public UserService(MusicAppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }
        public async Task<int> GetUsersCountAsync()
        {
            int users = await dbContext
                .Users
                .CountAsync();

            return users;
        }
        public async Task<IEnumerable<UserManagementIndexViewModel>> GetAllUsersAsync(string? searchTerm, string? roleFilter)
        {
            var query = dbContext.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query
                    .Where(u => u.Email.Contains(searchTerm) || u.UserName.Contains(searchTerm));
            }

            var usersList = await query.ToListAsync();

            if (!string.IsNullOrEmpty(roleFilter))
            {
                var filteredUsers = new List<ApplicationUser>();

                foreach (var user in usersList)
                {
                    var roles = await userManager.GetRolesAsync(user);
                    bool include = false;

                    if (roleFilter.ToLower() == "admin")
                    {
                        include = roles.Contains("Admin");
                    }
                    else if (roleFilter.ToLower() == "user")
                    {
                        include = !roles.Contains("Admin");
                    }

                    if (include)
                    {
                        filteredUsers.Add(user);
                    }
                }
                usersList = filteredUsers;
            }

            var viewModels = new List<UserManagementIndexViewModel>();
            foreach (var user in usersList)
            {
                var isAdmin = await userManager.IsInRoleAsync(user, "Admin");

                viewModels.Add(new UserManagementIndexViewModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    IsAdmin = isAdmin,
                    IsActive = !user.LockoutEnabled || user.LockoutEnd <= DateTimeOffset.Now
                });
            }

            return viewModels;
        }

        public async Task MakeAdminAsync(string userId)
        {
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                bool isInRole = await userManager.IsInRoleAsync(user, "Admin");

                if (!isInRole)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

        public async Task RemoveAdminAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null && await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.RemoveFromRoleAsync(user, "Admin");
            }
        }

        public async Task ToggleActivationAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user != null)
            {
                if (user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.Now)
                {
                    user.LockoutEnd = DateTimeOffset.Now;
                    user.LockoutEnabled = false;
                }
                else
                {
                    user.LockoutEnd = DateTimeOffset.MaxValue;
                    user.LockoutEnabled = true;
                }
                await userManager.UpdateAsync(user);
            }
        }
    }
}
