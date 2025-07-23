using CinemaApp.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Web.ViewModels.Notification;
using TagLib.Ape;

namespace MusicApp.Web.Controllers
{
    public class NotificationsController : BaseController
    {
        private readonly INotificationsService notificationsService;

        public NotificationsController(INotificationsService notificationsService)
        {
            this.notificationsService = notificationsService;
        }

        public async Task<IActionResult> Index(string? filter)
        {
            IEnumerable<NotificationViewModel> notifications = await notificationsService
                .GetAllNotificationsAsync(GetUserId()!, filter);

            return View(notifications);
        }

        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await notificationsService.MarkAsReadAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
