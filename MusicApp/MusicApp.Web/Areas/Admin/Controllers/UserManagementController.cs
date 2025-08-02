using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Admin.UserManagement;

namespace MusicApp.Web.Areas.Admin.Controllers
{
    public class UserManagementController : BaseAdminController
    {
        private readonly IUserService userService;
        private readonly UserManager<ApplicationUser> userManager;

        public UserManagementController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            this.userService = userService;
            this.userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<UserManagementIndexViewModel> users= await userService
                .GetAllUsersAsync();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (await userManager.IsInRoleAsync(user, "Admin"))
                await userService.RemoveAdminAsync(userId);
            else
                await userService.MakeAdminAsync(userId);

            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //public async Task<IActionResult> ToggleBan(string userId)
        //{
        //   ApplicationUser? user =await userManager.FindByIdAsync(userId);

           
        //    Console.WriteLine($"User: {user?.UserName}, IsDeleted: {user?.IsDeleted}!!!!!!!!!!!!!!!!!!!!!!!!!!!!");


        //    return RedirectToAction(nameof(Index));
        //}

    }
}
