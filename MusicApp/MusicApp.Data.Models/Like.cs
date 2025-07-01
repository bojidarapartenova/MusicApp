using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Data.Models
{
    public class Like
    {
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;
        public Guid SongId { get; set; }
        [Required]
        [ForeignKey(nameof(SongId))]
        public Song Song { get; set; } = null!;
    }
}
