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
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Admin.UserManagement;

namespace MusicApp.Services.Core
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
        public async Task<IEnumerable<UserManagementIndexViewModel>> GetAllUsersAsync()
        {
            var users = userManager.Users.ToList();
            var userViewModels = new List<UserManagementIndexViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                bool isAdmin = roles.Contains("Admin");

                userViewModels.Add(new UserManagementIndexViewModel()
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    IsAdmin = isAdmin
                });
            }

            return userViewModels;
        }

        public async Task MakeAdminAsync(string userId)
        {
            ApplicationUser? user= await userManager.FindByIdAsync(userId);

            if(user!=null)
            {
                bool isInRole = await userManager.IsInRoleAsync(user, "Admin");

                if(!isInRole)
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

        //public async Task SoftDeleteAsync(string userId)
        //{
        //    ApplicationUser? user=await userManager.FindByEmailAsync(userId);

        //    if(user!=null)
        //    {
        //        user.IsDeleted = true;
        //        await userManager.UpdateAsync(user);
        //    }
        //}

        //public async Task RestoreUserAsync(string userId)
        //{
        //    ApplicationUser? user = await userManager.FindByEmailAsync(userId);

        //    if (user != null)
        //    {
        //        user.IsDeleted = false;
        //        await userManager.UpdateAsync(user);
        //    }
        //}
    }
}
