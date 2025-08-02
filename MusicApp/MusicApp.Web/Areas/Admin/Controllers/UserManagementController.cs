using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Models;
using MusicApp.Services.Core;
using MusicApp.Services.Core.Admin.Interfaces;
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
        public async Task<IActionResult> Index(string? searchTerm, string? roleFilter)
        {
            IEnumerable<UserManagementIndexViewModel> users= await userService
                .GetAllUsersAsync(searchTerm, roleFilter);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return RedirectToAction("Index");
                }

                ApplicationUser? user=await userManager.FindByIdAsync(userId);

                if(user != null)
                {
                    if(await userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await userService.RemoveAdminAsync(userId);
                    }
                    else
                    {
                        await userService.MakeAdminAsync(userId);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction("Index");   
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActivation(string userId)
        {
            try
            {
                await userService.ToggleActivationAsync(userId);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
