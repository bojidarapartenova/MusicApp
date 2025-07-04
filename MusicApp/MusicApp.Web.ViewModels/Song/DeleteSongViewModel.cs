using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApp.Web.ViewModels.Song
{
    public class DeleteSongViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;

        [Required]
        public string PublisherId { get; set; } = null!;

        public string? Publisher { get; set; } = null!;

    }
}
