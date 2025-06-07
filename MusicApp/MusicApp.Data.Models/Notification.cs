using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SongId { get; set; }

        [ForeignKey(nameof(SongId))]
        public Song Song { get; set; } = null!;
    }
}
