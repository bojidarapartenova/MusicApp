using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MusicApp.Data.Models.Enums;

namespace MusicApp.Data.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public string AuthorId { get; set; } = null!;
        [ForeignKey(nameof(AuthorId))]
        public ApplicationUser Author { get; set; } = null!;

        [Required]
        public Guid SongId { get; set; }
        [ForeignKey(nameof(SongId))]
        public Song Song { get; set; } = null!;

        public Guid? CommentId { get; set; }
        [ForeignKey(nameof(CommentId))]
        public Comment? Comment { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        [MaxLength(256)]
        public string? Message { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
