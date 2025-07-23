using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Data.Models.Enums;

namespace MusicApp.Web.ViewModels.Notification
{
    public class NotificationViewModel
    {
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid SongId { get; set; }
        public string SongTitle { get; set; } = null!;
        public string? SongImageUrl;

        public string AuthorId { get; set; } = null!;
        public string Author { get; set; } = null!;

        public Guid? CommentId { get; set; }
        public string? Message { get; set; }
    }

}
