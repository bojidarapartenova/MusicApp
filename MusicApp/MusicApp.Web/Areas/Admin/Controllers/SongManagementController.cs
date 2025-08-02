using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Services.Core.Admin.Interfaces;

namespace MusicApp.Web.Areas.Admin.Controllers
{
    public class SongManagementController : BaseAdminController
    {
        private readonly ISongManagementService songManagementService;

        public SongManagementController(ISongManagementService songManagementService)
        {
            this.songManagementService = songManagementService;
        }
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var songs = await songManagementService.GetAllSongsAsync(searchTerm);
            ViewData["CurrentFilter"] = searchTerm;
            return View(songs);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index));
                }

                await songManagementService.SoftDeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Restore(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index));
                }

                await songManagementService.RestoreAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
