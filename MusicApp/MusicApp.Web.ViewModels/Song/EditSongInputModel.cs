﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Song
{
    public class EditSongInputModel
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = null!;

        [Required]
        public string PublisherId { get; set; } = null!;

        [Required]
        public int GenreId { get; set; }
        public IEnumerable<AddSongGenreDropDown>? Genres {  get; set; }

        [Required]
        [MaxLength(100)]
        public string Artist { get; set; } = null!;

        public int Duration { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Likes { get; set; }
        public string? ImageUrl { get; set; }
        [Required]
        public string AudioUrl { get; set; } = null!;
    }
}
