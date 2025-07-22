using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Web.ViewModels.Notification;

namespace MusicApp.Services.Core.Interfaces
{
    public interface INotificationsService
    {
        Task NotifySongLikedAsync(Guid songId, string authorId);
        Task NotifyCommentAsync(Guid songId, Guid commentId, string message, string authorId);
        Task<IEnumerable<NotificationViewModel>> GetAllNotificationsAsync(string userId);
        Task MarkAsReadAsync(Guid notificationId);
    }
}
