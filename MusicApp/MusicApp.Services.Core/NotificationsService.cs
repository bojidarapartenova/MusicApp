using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data.Data;
using MusicApp.Data.Models;
using MusicApp.Services.Core.Interfaces;
using MusicApp.Data.Models.Enums;
using MusicApp.Web.ViewModels.Notification;

namespace MusicApp.Services.Core
{
    public class NotificationsService:INotificationsService
    {
        private readonly MusicAppDbContext dbContext;

        public NotificationsService(MusicAppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task NotifySongLikedAsync(Guid songId, string authorId)
        {
            Song? song=await dbContext
                .Songs
                .FirstOrDefaultAsync(s=>s.Id == songId);

            if(song!=null && song.PublisherId!=authorId)
            {
                Notification notification = new Notification()
                {
                    UserId = song.PublisherId,
                    AuthorId = authorId,
                    SongId = songId,
                    Type = NotificationType.Like
                };
                dbContext.Notifications.Add(notification);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task NotifyCommentAsync(Guid songId, Guid commentId, string message, string authorId)
        {
            Song? song=await dbContext
                .Songs
                .FirstOrDefaultAsync(s=>s.Id==songId);

            if(song!=null && song.PublisherId!=authorId)
            {
                Notification notification = new Notification
                {
                    UserId = song.PublisherId,
                    SongId=songId,
                    AuthorId = authorId,
                    CommentId = commentId,
                    Type = NotificationType.Comment,
                    Message = message
                };
                dbContext.Notifications.Add(notification);
                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NotificationViewModel>> GetAllNotificationsAsync(string userId)
        {
            IEnumerable<NotificationViewModel> notifications = await dbContext
                .Notifications
                .AsNoTracking()
                .Where(n=>n.UserId.ToLower()==userId.ToLower())
                .OrderByDescending(n=>n.CreatedAt)
                .Select(n => new NotificationViewModel()
                {
                    Id = n.Id,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    SongId = n.SongId,
                    SongTitle = n.Song.Title,
                    AuthorId = n.AuthorId,
                    Author = n.Author.UserName!,
                    CommentId = n.CommentId,
                    Message = n.Message
                })
                .ToArrayAsync();

            return notifications;
        }
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            Notification? notification = await dbContext
                .Notifications
                .FindAsync(notificationId);

            if(notification!=null)
            {
                notification.IsRead = true;
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
