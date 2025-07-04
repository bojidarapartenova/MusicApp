﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SongId { get; set; }

        [Required]
        [ForeignKey(nameof(SongId))]
        public Song Song { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Text { get; set; } = null!;
    }
}
